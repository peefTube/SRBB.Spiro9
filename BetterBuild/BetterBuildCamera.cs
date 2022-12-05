using BetterBuild.Gizmo;
using BetterBuild.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterBuild
{
    public class BetterBuildCamera : MonoBehaviour
    {
        public static BetterBuildCamera Instance { get; private set; }

        private static GameObject m_Camera;
        private static GameObject m_GizmoHandle;

        public Camera EditorCamera;

        public static void CreateCamera()
        {
            if (m_Camera != null) return;

            m_Camera = new GameObject("BetterBuildCamera");
            var cam = m_Camera.AddComponent<Camera>();
            var bbcam = m_Camera.AddComponent<BetterBuildCamera>();

            m_GizmoHandle = new GameObject("HandleObject");
            GizmoObject gizmo = m_GizmoHandle.AddComponent<GizmoObject>();
            gizmo.cam = cam;

            bbcam.SetActive(true);
        }

        public static void DestroyCamera()
        {
            if (m_Camera == null) return;

            Destroy(m_Camera);
        }

        private void Start()
        {
            Instance = this;
            EditorCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            transform.position = SRSingleton<SceneContext>.Instance.Player.transform.position;
            
            SRSingleton<SceneContext>.Instance.Player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            SRSingleton<SceneContext>.Instance.Player.GetComponent<vp_FPController>().MotorFreeFly = true;
            foreach (Transform playerObj in SRSingleton<SceneContext>.Instance.Player.transform.GetChild(0))
                playerObj.gameObject.SetActive(false);
            foreach (var spawner in Resources.FindObjectsOfTypeAll<DirectedActorSpawner>())
                spawner.enabled = false;

            RenderSettings.fog = Globals.Settings.EnableFog;

            FindObjectOfType<vp_FPInput>().AllowGameplayInput = false;

            //Camera.main.transform.GetChild(0).gameObject.SetActive(false);
            //FindObjectOfType<SetMouseEnabled>().gameObject.SetActive(false);
            HudUI.Instance.gameObject.SetActive(false);

            SRInput.Instance.SetInputMode(SRInput.InputMode.NONE);
            //SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>().vacMode = WeaponVacuum.VacMode.GADGET;
            SRSingleton<SceneContext>.Instance.PlayerState.InGadgetMode = true;

            StartCoroutine(DestroyDelayed());
        }

        private IEnumerator DestroyDelayed()
        {
            while (gameObject.activeSelf)
            {
                foreach (var identifiable in Resources.FindObjectsOfTypeAll<Identifiable>())
                {
                    if (identifiable.id == Identifiable.Id.PLAYER) continue;
                    try
                    {
                        Destroyer.DestroyActor(identifiable.gameObject, "CameraActivated");
                    }
                    catch { }
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void OnDisable()
        {
            SRSingleton<SceneContext>.Instance.Player.transform.position = transform.position;
            SRSingleton<SceneContext>.Instance.Player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            SRSingleton<SceneContext>.Instance.Player.GetComponent<vp_FPController>().MotorFreeFly = false;
            foreach (Transform playerObj in SRSingleton<SceneContext>.Instance.Player.transform.GetChild(0))
                playerObj.gameObject.SetActive(true);
            foreach (var spawner in Resources.FindObjectsOfTypeAll<DirectedActorSpawner>())
                spawner.enabled = true;

            GameObject.FindObjectOfType<vp_FPInput>().AllowGameplayInput = true;
            GameObject.FindObjectOfType<vp_FPInput>().MouseCursorForced = false;
            RenderSettings.fog = true;

            //Camera.main.transform.GetChild(0).gameObject.SetActive(true);
            //FindObjectOfType<SetMouseEnabled>().gameObject.SetActive(true);
            HudUI.Instance.gameObject.SetActive(true);

            SRInput.Instance.SetInputMode(SRInput.InputMode.DEFAULT);
            //SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>().vacMode = WeaponVacuum.VacMode.NONE;
            SRSingleton<SceneContext>.Instance.PlayerState.InGadgetMode = false;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            ToolbarUI.Instance.gameObject.SetActive(active);
            SettingsUI.Instance.gameObject.SetActive(active);
            HierarchyUI.Instance.gameObject.SetActive(active);
            TeleportUI.Instance.gameObject.SetActive(active);
            InfoUI.Instance.gameObject.SetActive(active);
        }

        private float speed = 1;
        public float speedH = 2.0f;
        public float speedV = 2.0f;
        private float yaw = 0.0f;
        private float pitch = 0.0f;

        void Update()
        {
            if (SRSingleton<SceneContext>.Instance.PopupDirector.currPopup != null)
            {
                MonoBehaviour ui = (MonoBehaviour)SRSingleton<SceneContext>.Instance.PopupDirector.currPopup;
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                    SRSingleton<SceneContext>.Instance.PopupDirector.currPopup = null;
                }
            }
            if (SRSingleton<SceneContext>.Instance.TutorialDirector.currPopup != null)
            {
                Destroy(SRSingleton<SceneContext>.Instance.TutorialDirector.currPopup.gameObject);
                SRSingleton<SceneContext>.Instance.PopupDirector.currPopup = null;
            }

            SRSingleton<SceneContext>.Instance.Player.transform.position = transform.position - transform.forward;

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    speed = Mathf.Clamp(speed - 0.1f, 0.1f, 10);
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    speed = Mathf.Clamp(speed + 0.1f, 0.1f, 10);
                }
            }

            //if (Input.GetKeyDown(KeyCode.LeftArrow))
            //{
            //    var regionSets = (RegionRegistry.RegionSetId[])Enum.GetValues(typeof(RegionRegistry.RegionSetId));
            //    if (regionIndex <= 0)
            //        regionIndex = regionSets.Length;
            //    regionIndex--;
            //    regionSet = regionSets[regionIndex];
            //    SRSingleton<SceneContext>.Instance.PlayerState.model.SetCurrRegionSet(regionSet);
            //}
            //if (Input.GetKeyDown(KeyCode.RightArrow))
            //{
            //    var regionSets = (RegionRegistry.RegionSetId[])Enum.GetValues(typeof(RegionRegistry.RegionSetId));
            //    regionIndex++;
            //    if (regionIndex >= regionSets.Length)
            //        regionIndex = 0;
            //    regionSet = regionSets[regionIndex];
            //    SRSingleton<SceneContext>.Instance.PlayerState.model.SetCurrRegionSet(regionSet);
            //}

            if(Input.GetMouseButtonDown(1))
            {
                yaw = transform.eulerAngles.y;
            }

            if (Input.GetMouseButton(1))
            {
                FindObjectOfType<vp_FPInput>().MouseCursorForced = false;

                yaw += speedH * Input.GetAxis("Mouse X");
                pitch -= speedV * Input.GetAxis("Mouse Y");
                pitch = Mathf.Clamp(pitch, -90, 90);

                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
            else
            {
                FindObjectOfType<vp_FPInput>().MouseCursorForced = true;
            }

            if(Input.GetKey(KeyCode.LeftControl) || EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * speed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * speed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * speed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * speed;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                transform.position += Vector3.up * speed;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.position += Vector3.down * speed;
            }

            if (transform.hasChanged)
            {
                GizmoObject.Instance.OnCameraMove();
            }
        }
    }
}
