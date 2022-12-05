using System.Collections;
using System.Collections.Generic;

public class UndoManager : SRSingleton<UndoManager>
{
    /**
		 * Stores a summary of the action, the target (IUndo inheriting object), and a hashtable of values representing
		 * object state at time of undo snapshot.
		 */
    protected class UndoState
    {
        // A summary of the undo action.
        public string message;

        // The object that is targeted by this action.
        public IUndo target;

        // A collection of values representing the state of `target`.  Will by passed to IUndo::ApplyState.
        public Hashtable values;

        /**
         * Initialize a new UndoState object with an IUndo object and summary of the undo-able action.
         */
        public UndoState(IUndo target, string msg)
        {
            this.target = target;
            this.message = msg;
            this.values = target.RecordState();
        }

        /**
         * Reverts the IUndo state by calling IUndo::ApplyState()
         */
        public void Apply()
        {
            target.ApplyState(values);
        }

        /**
         * Returns the summary of this undo action.
         */
        public override string ToString()
        {
            return message;
        }
    }

    public Callback undoPerformed = null;
    public Callback redoPerformed = null;
    public Callback undoStackModified = null;
    public Callback redoStackModified = null;

    /**
		 * Add a callback when an Undo action is performed.
		 */
    public static void AddUndoPerformedListener(Callback callback)
    {
        if (Instance.undoPerformed != null)
            Instance.undoPerformed += callback;
        else
            Instance.undoPerformed = callback;
    }

    /**
     * Add a callback when an Redo action is performed.
     */
    public static void AddRedoPerformedListener(Callback callback)
    {
        if (Instance.redoPerformed != null)
            Instance.redoPerformed += callback;
        else
            Instance.redoPerformed = callback;
    }

    /**
		 * Add a callback when an Undo action is performed.
		 */
    public static void AddUndoChangedListener(Callback callback)
    {
        if (Instance.undoStackModified != null)
            Instance.undoStackModified += callback;
        else
            Instance.undoStackModified = callback;
    }

    /**
     * Add a callback when an Redo action is performed.
     */
    public static void AddRedoChangedListener(Callback callback)
    {
        if (Instance.redoStackModified != null)
            Instance.redoStackModified += callback;
        else
            Instance.redoStackModified = callback;
    }

    private Stack<List<UndoState>> undoStack = new Stack<List<UndoState>>();
    private Stack<List<UndoState>> redoStack = new Stack<List<UndoState>>();

    /**
     * Return a formatted string with a summary of every undo-able action in the undo stack.
     */
    public static string PrintUndoStack()
    {
        return Instance.PrintStack(Instance.undoStack);
    }

    /**
     * Return a formatted string with a summary of every redo-able action in the redo stack.
     */
    public static string PrintRedoStack()
    {
        return Instance.PrintStack(Instance.redoStack);
    }

    /**
     * Create a nicely formatted string with a stack.
     */
    private string PrintStack(Stack<List<UndoState>> stack)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (List<UndoState> collection in stack)
        {
            foreach (UndoState state in collection)
            {
                sb.AppendLine(state.ToString());
                break;
            }

            sb.AppendLine("-----");
        }

        return sb.ToString();
    }

    UndoState currentUndo, currentRedo;

    private void PushUndo(List<UndoState> state)
    {
        currentUndo = state[0];
        undoStack.Push(state);

        if (undoStackModified != null)
            undoStackModified();
    }

    private void PushRedo(List<UndoState> state)
    {
        currentRedo = state[0];
        redoStack.Push(state);

        if (redoStackModified != null)
            redoStackModified();
    }

    private List<UndoState> PopUndo()
    {
        List<UndoState> states = Pop(undoStack);

        if (states == null || undoStack.Count < 1)
            currentUndo = null;
        else
            currentUndo = ((List<UndoState>)undoStack.Peek())[0];

        if (undoStackModified != null)
            undoStackModified();

        return states;
    }

    private List<UndoState> PopRedo()
    {
        List<UndoState> states = Pop(redoStack);

        if (states == null || redoStack.Count < 1)
            currentRedo = null;
        else
            currentRedo = ((List<UndoState>)redoStack.Peek())[0];

        if (redoStackModified != null)
            redoStackModified();

        return states;
    }

    private List<UndoState> Pop(Stack<List<UndoState>> stack)
    {
        if (stack.Count > 0)
            return (List<UndoState>)stack.Pop();
        else
            return null;
    }

    private static void ClearStack(Stack<List<UndoState>> stack)
    {
        foreach (List<UndoState> commands in stack)
            foreach (UndoState state in commands)
                state.target.OnExitScope();

        stack.Clear();
    }

    /**
     * Get the message from the last registered IUndo, or if PerformRedo was called more recently,
     * this will be the currently queued undo action.
     */
    public static string GetCurrentUndo()
    {
        return Instance.currentUndo == null ? "" : Instance.currentUndo.message;
    }

    /**
     * Get the message accompanying the next queued redo action.
     */
    public static string GetCurrentRedo()
    {
        return Instance.currentRedo == null ? "" : Instance.currentRedo.message;
    }

    public static int GetRedoCount()
    {
        return Instance.redoStack.Count;
    }

    public static int GetUndoCount()
    {
        return Instance.undoStack.Count;
    }

    /**
     * Register a new undoable state with message.  Message should describe the action that will be
     * undone.
     * \sa IUndo
     */
    public static void RegisterState(IUndo target, string message)
    {
        ClearStack(Instance.redoStack);
        if (Instance.redoStackModified != null)
            Instance.redoStackModified();

        Instance.currentRedo = null;
        Instance.PushUndo(new List<UndoState>() { new UndoState(target, message) });
    }

    /**
     * Register a collection of undoable states with message.  Message should describe the action that
     * will be undone.
     * \sa IUndo
     */
    public static void RegisterStates(IEnumerable<IUndo> targets, string message)
    {
        ClearStack(Instance.redoStack);
        if (Instance.redoStackModified != null)
            Instance.redoStackModified();

        Instance.currentRedo = null;
        List<UndoState> states = new List<UndoState>();
        foreach (IUndo target in targets)
            states.Add(new UndoState(target, message));
        Instance.PushUndo(states);
    }

    /**
     * Applies the currently queued Undo state.
     */
    public static void PerformUndo()
    {
        List<UndoState> states = Instance.PopUndo();

        if (states == null)
            return;

        List<UndoState> redostates = new List<UndoState>();
        foreach (UndoState state in states)
            redostates.Add(new UndoState(state.target, state.message));

        Instance.PushRedo(redostates);

        foreach (UndoState state in states)
        {
            state.Apply();
        }

        if (Instance.undoPerformed != null)
            Instance.undoPerformed();
    }

    /**
     * If the Redo stack exists, this applies the queued Redo action.  Redo is cleared on Undo.RegisterState 
     * or Undo.RegisterStates calls.
     */
    public static void PerformRedo()
    {
        List<UndoState> states = Instance.PopRedo();

        if (states == null)
            return;

        List<UndoState> undostates = new List<UndoState>();
        foreach (UndoState state in states)
            undostates.Add(new UndoState(state.target, state.message));

        Instance.PushUndo(undostates);

        foreach (UndoState state in states)
            state.Apply();

        if (Instance.redoPerformed != null)
            Instance.redoPerformed();
    }
}
