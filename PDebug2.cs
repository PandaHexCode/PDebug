using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Text;
using System.IO;

public class PDebug2 : MonoBehaviour {

    public static PDebug2 instance = null;

    private string NAME = "PDebug";
    private int currentState = 0;/*0 = Console , 1 = Objects , 2 = Change , 3 = Components , 4 = Time , 5 = Childrens , 6 = TargetCamera , 7 = Scene , 8 = LoadScene, etc...*/
    private List<string> consoleLogs = new List<string>();/*Console Log*/
    private GameObject targetObject = null;/*Current GameObject in Objects Tab*/
    private Camera targetCamera = null;/*Camera that calculating the Ray*/
    private UnityEngine.Component targetComponent = null;
    private MethodInfo targetMethod;

    private bool is3D = false;/*This Script uses a other Ray for 3D Games!*/
    private bool logExceptions = false;/*True needs lot more Performance!*/

    private static PDebugTCP tcp = null;
    private static FreeCamController freeCam = null;

    private void Awake() {
        this.targetCamera = Camera.main;

        /*Check if an instance allready exits and if exits destroy this else create a new gameobject with the instance*/
        if (PDebug2.instance == null) {
            if (this.gameObject.name != this.NAME + "-INSTANCE") {
                GameObject gm = new GameObject(this.NAME + "-INSTANCE");
                PDebug2.instance = gm.AddComponent<PDebug2>();
                PDebug2.tcp = gm.AddComponent<PDebugTCP>();
                PDebug2.tcp.StartServer();
                DontDestroyOnLoad(tcp);
                DontDestroyOnLoad(instance);
                Destroy(this);
            }
        }
        else if (PDebug2.instance != this)
            Destroy(this);

        Application.logMessageReceived += HandleLog;/*Register Console Log*/
    }

    private void Update() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (this.currentState == 1) {
            if (Input.GetKeyDown(KeyCode.I)) {/*Try to get a TargetObject for the Objects Tab*/
                if (this.targetCamera == null)
                    this.targetCamera = Camera.main;

                Ray ray = this.targetCamera.ScreenPointToRay(Input.mousePosition);
                if (!this.is3D) {/*2D*/
                    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                    if (hit.collider != null)
                        this.targetObject = hit.collider.gameObject;
                }
                else {/*3D*/
                    RaycastHit hit;
                    bool old = Physics.queriesHitTriggers;
                    Physics.queriesHitTriggers = false;
                    if (Physics.Raycast(ray, out hit)) {
                        if (hit.collider != null)
                            this.targetObject = hit.collider.gameObject;
                    }

                    Physics.queriesHitTriggers = old;
                }
            }
        }
    }

    private void OnGUI() {
        DrawMainButtons();

        switch (this.currentState) {
            case 0:
                DrawConsole();
                break;
            case 1:
                DrawObjects();
                break;
            case 2:
                DrawChange();
                break;
            case 3:
                DrawComponents();
                break;
            case 4:
                DrawTimeScreen();
                break;
            case 5:
                DrawChildrens();
                break;
            case 6:
                DrawTargetCamera();
                break;
            case 7:
                DrawScene();
                break;
            case 8:
                DrawLoadScene();
                break;
            case 9:
                DrawApplication();
                break;
            case 10:
                DrawCompValues();
                break;
            case 11:
                DrawCompMethodes();
                break;
            case 12:
                DrawInvokeWithParameters();
                break;
            case 13:
                DrawTCPMenu();
                break;
            case 14:
                DrawOther();
                break;
            case 15:
                DrawObjectLayer();
                break;
            case 16:
                DrawResources();
                break;
            case 17:
                DrawCustomDll();
                break;
            case 18:              
                DrawPhysics();
                break;
        }
    }

    private void DrawMainButtons() {
        if (GUI.Button(new Rect(5f, 155f, 70f, 20f), "Console"))
            this.currentState = 0;
        if (GUI.Button(new Rect(80f, 155f, 70f, 20f), "Objects"))
            this.currentState = 1;
        if (GUI.Button(new Rect(155f, 155f, 70f, 20f), "Time"))
            this.currentState = 4;
        if (GUI.Button(new Rect(230f, 155f, 70f, 20f), "Scene"))
            this.currentState = 7;
        if (GUI.Button(new Rect(305f, 155f, 70f, 20f), "TCP"))
            this.currentState = 13;
        if (GUI.Button(new Rect(5f, 180f, 70f, 20f), "Other"))
            this.currentState = 14;
        if (GUI.Button(new Rect(80f, 180f, 80f, 20f), "Application"))
            this.currentState = 9;
    }

    private string dllInput = @"D:\C#\PDebugCustomDLLs\";
    private string typeInput = "CustomDLLTest.TestBehaviour";
    private void DrawCustomDll() {
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - CutomDll");
        GUI.Label(new Rect(10f, 30f, 200f, 50f), "DLLPath, Namespace.BehaviourName");
        this.dllInput = GUI.TextField(new Rect(10f, 65f, 250, 18), this.dllInput);
        this.typeInput = GUI.TextField(new Rect(10f, 85f, 250, 18), this.typeInput);
        if (GUI.Button(new Rect(10f, 115f, 100f, 20f), "Inject Type"))
            AddTypeFromAssembly(this.dllInput, this.typeInput, new GameObject("CustomDLLTypeHost"));
        if (GUI.Button(new Rect(120f, 115f, 180f, 20f), "Inject Type to TargetObject"))
            AddTypeFromAssembly(this.dllInput, this.typeInput, this.targetObject);
    }

    private string[] physicsInputs = new string[15];
    private void DrawPhysics() {
        GUI.Box(new Rect(0f, 0f, 500f, 150f), this.NAME + " - Physics");
        if (GUI.Button(new Rect(10f, 2.5f, 100f, 20f), "Reload")) {
            if (this.is3D){
                this.physicsInputs[0] = Physics.gravity.x.ToString();
                this.physicsInputs[1] = Physics.gravity.y.ToString();
                this.physicsInputs[2] = Physics.gravity.z.ToString();
            }else{
                this.physicsInputs[0] = Physics2D.gravity.x.ToString();
                this.physicsInputs[1] = Physics2D.gravity.y.ToString();
                this.physicsInputs[2] = "3D";
            }
        }
        if (GUI.Button(new Rect(112f, 2.5f, 80f, 20f), "Apply")){
            if (this.is3D){
                Physics.gravity = new Vector3(StringToFloat(this.physicsInputs[0]), Physics.gravity.y, Physics.gravity.z);
                Physics.gravity = new Vector3(Physics.gravity.x, StringToFloat(this.physicsInputs[1]), Physics.gravity.z);
                Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y,StringToFloat(this.physicsInputs[2]));
            }else{
                Physics2D.gravity = new Vector3(StringToFloat(this.physicsInputs[0]), Physics2D.gravity.y);
                Physics2D.gravity = new Vector3(Physics2D.gravity.x, StringToFloat(this.physicsInputs[1]));
            }
        }

        GUI.Label(new Rect(10f, 30f, 400f, 50f), "Change Physics settings of ProjectSettings (Is3D " + this.is3D + ")");
        this.physicsInputs[0] = GUI.TextField(new Rect(10f, 60f, 75f, 20f), "GravityX " + this.physicsInputs[0]).Split(' ')[1];
        this.physicsInputs[1] = GUI.TextField(new Rect(90f, 60f, 75f, 20f), "GravityY " + this.physicsInputs[1]).Split(' ')[1];
        this.physicsInputs[2] = GUI.TextField(new Rect(170f, 60f, 75f, 20f), "GravityZ " + this.physicsInputs[2]).Split(' ')[1];

        if (this.is3D){
            if (GUI.Button(new Rect(250f, 60f, 195f, 20f), "Queries Hit Triggers: " + Physics.queriesHitTriggers))
                Physics.queriesHitTriggers = !Physics.queriesHitTriggers;
        }else{
            if (GUI.Button(new Rect(250f, 60f, 195f, 20f), "Queries Hit Triggers: " + Physics2D.queriesHitTriggers))
                Physics2D.queriesHitTriggers = !Physics2D.queriesHitTriggers;
        }
    }

    public void AddTypeFromAssembly(string dllPath, string typeName, GameObject go){
        if (go == null)
            Debug.Log("TargetObject is null!");
        Assembly simpleasm = Assembly.LoadFrom(dllPath);
        System.Type testBehaviour = simpleasm.GetType(typeName);
        Assembly.UnsafeLoadFrom(dllPath);
        go.AddComponent(testBehaviour);
        Debug.Log("Successfully injected " + typeName + " from " + dllPath + " to " + go + "!");
    }

    private void DrawConsole(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME+" - Console");

        if (GUI.Button(new Rect(300, 128f, 70f, 20f), "Clear"))
            this.consoleLogs.Clear();
        if (GUI.Button(new Rect(300, 108f, 70f, 20f), "Last"))
            this.scroolSectionBeginValue = consoleLogs.Count-1;
        if (GUI.Button(new Rect(300, 88f, 70f, 20f), "First"))
            this.scroolSectionBeginValue = 0;

        DrawScroolSection(1, 1, this.consoleLogs.Count, 0, 6, 0, true);
    }

    private void DrawConsoleSection(float lastY, int i){
        GUI.Label(new Rect(10f, 50f, 390f, 100f), this.consoleLogs[i]);
    }

    private void DrawScene(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Scene");
        GUI.Label(new Rect(10f, 30f, 200f, 50f), "ActiveScene: "+SceneManager.GetActiveScene().name);
        if (GUI.Button(new Rect(10f, 70f, 85f, 20f), "Load Scene"))
            this.currentState = 8;
    }

    private GameObject oldCamera;
    private void DrawOther(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Other");

        float fps = (int)(1f / Time.unscaledDeltaTime);

        GUI.Label(new Rect(10f, 30f, 200f, 50f), "FPS: " + fps);

        if (GUI.Button(new Rect(10f, 70f, 85f, 20f), "Resources"))
            this.currentState = 16;
        if (GUI.Button(new Rect(10f, 100f, 85f, 20f), "Physics"))
            this.currentState = 18;

        string freeCamString = "FreeCam";
        if(PDebug2.freeCam != null)
            freeCamString = "Disbable";

        if (GUI.Button(new Rect(100f, 70f, 85f, 20f), freeCamString)){
            if (PDebug2.freeCam != null){
                Destroy(PDebug2.freeCam.gameObject);
                this.oldCamera.gameObject.SetActive(true);
                return;
            }
            if (this.targetCamera == null)
                this.targetCamera = Camera.main;
            GameObject clon = Instantiate(this.targetCamera.gameObject);
            foreach (var comp in clon.GetComponents<Component>()){
                if (!(comp is Transform) && !(comp is Camera))
                    Destroy(comp);
            }
            this.oldCamera = this.targetCamera.gameObject;
            this.oldCamera.SetActive(false);
            PDebug2.freeCam = clon.AddComponent<FreeCamController>();
            this.targetCamera = clon.GetComponent<Camera>();
            this.targetCamera.Render();
           // this.targetCamera.tag = "Main Camera";
            this.targetObject = this.targetCamera.gameObject;
            clon.name = "FreeCam";
            PDebug2.freeCam.gameObject.SetActive(true);
        }

        if (GUI.Button(new Rect(190f, 70f, 85f, 20f), "CustomDLL"))
            this.currentState = 17;
    }

    public void DrawResources(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Resources");
        List<GameObject> resources = new List<GameObject>();
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject))){
            if (obj.scene.name == null && obj.transform.parent == null)
                resources.Add(obj);
        }
        DrawScroolSection(0, 3, resources.Count, 0, 7, 14);
    }

    public void DrawResourcesSection(float lastY, int i){
        List<GameObject> resources = new List<GameObject>();
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject))){
            if (obj.scene.name == null && obj.transform.parent == null)
                resources.Add(obj);
        }

        GUI.Label(new Rect(10f, lastY, 300f, 30f), resources[i].name);

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Edit")){
            this.targetObject = resources[i];
            this.currentState = 1;
        }
    }

    private void DrawLoadScene(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Load Scene");

        DrawScroolSection(1, 3, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings, 0, 0);
    }

    private void DrawLoadSceneSection(float lastY, int i){
        string sceneName = i.ToString();

        GUI.Label(new Rect(10f, lastY, 300f, 30f), sceneName);

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Load"))
            SceneManager.LoadScene(i);
    }

    private void DrawApplication(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Application");
        GUI.Label(new Rect(10f, 30f, 200f, 50f), "RunInBackground: " + Application.runInBackground);
        GUI.Label(new Rect(10f, 50f, 200f, 50f), "UnityVersion: " + Application.unityVersion);
        GUI.Label(new Rect(10f, 120f, 200f, 50f), this.NAME+" by PandaHexCode");
        if (GUI.Button(new Rect(170f, 34f, 70f, 15f), "Switch"))
            Application.runInBackground = !Application.runInBackground;
        if (GUI.Button(new Rect(250f, 34f, 100f, 15f), "Is3D: "+this.is3D))
            this.is3D = !this.is3D;
    }

    private void DrawChildrens(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Childrens");

        DrawScroolSection(this.targetObject, 3, this.targetObject.transform.childCount, 0, 1);
    }

    private void DrawChildrensSection(float lastY, int i){
        GUI.Label(new Rect(10f, lastY, 300f, 30f), this.targetObject.transform.GetChild(i).name);

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Edit")){
            this.currentState = 1;
            this.targetObject = this.targetObject.transform.GetChild(i).gameObject;
            return;
        }
    }

    private void DrawCompValues(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Comp Values");

        DrawScroolSection(this.targetComponent, 3, this.targetComponent.GetType().GetProperties().Length, 0, 2, 3);
    }

    private void DrawCompValuesSection(float lastY, int i){
        PropertyInfo prop = this.targetComponent.GetType().GetProperties()[i];
        if (prop.Name.Equals("rigidbody") | prop.Name.Equals("rigidbody2D") | prop.Name.Equals("camera"))
            return;
        else{
            var value = prop.GetValue(this.targetComponent, null);

            GUI.Label(new Rect(10f, lastY, 300f, 25f), prop.Name + " - " + value);

            if (value.GetType().Equals(typeof(bool)))
            {
                if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Change"))
                    prop.SetValue(targetComponent, !(bool)value, null);
            }
            else if (value.GetType().Equals(typeof(int)))
            {
                if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "+"))
                    prop.SetValue(targetComponent, (int)value + 1, null);
                if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "-"))
                    prop.SetValue(targetComponent, (int)value - 1, null);
            }
            else if (value.GetType().Equals(typeof(float)))
            {
                if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "+"))
                    prop.SetValue(targetComponent, (float)value + 0.1f, null);
                if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "-"))
                    prop.SetValue(targetComponent, (float)value - 0.1f, null);
            }
            else if (value.GetType().Equals(typeof(Vector2)))
            {
                if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "x+"))
                    prop.SetValue(targetComponent, (Vector2)value + new Vector2(0.1f, 0), null);
                if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "x-"))
                    prop.SetValue(targetComponent, (Vector2)value + new Vector2(-0.1f, 0), null);
                if (GUI.Button(new Rect(270f, lastY, 30f, 20f), "y+"))
                    prop.SetValue(targetComponent, (Vector2)value + new Vector2(0, 0.1f), null);
                if (GUI.Button(new Rect(305f, lastY, 30f, 20f), "y-"))
                    prop.SetValue(targetComponent, (Vector2)value + new Vector2(0, -0.1f), null);
            }
            else if (value.GetType().Equals(typeof(Vector3)))
            {
                if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "x+"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(0.1f, 0, 0), null);
                if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "x-"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(-0.1f, 0, 0), null);
                if (GUI.Button(new Rect(270f, lastY, 30f, 20f), "y+"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(0, 0.1f, 0), null);
                if (GUI.Button(new Rect(305f, lastY, 30f, 20f), "y-"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(0, -0.1f, 0), null);
                if (GUI.Button(new Rect(340f, lastY, 30f, 20f), "z+"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(0, 0, 0.1f), null);
                if (GUI.Button(new Rect(375f, lastY, 30f, 20f), "z-"))
                    prop.SetValue(targetComponent, (Vector3)value + new Vector3(0, 0, -0.1f), null);
            }
        }
    }

    private void DrawCompMethodes(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Comp Methodes");

        DrawScroolSection(this.targetComponent, 3, this.targetComponent.GetType().GetMethods().Length, 0, 3, 3);
    }

    private void DrawCompMethodesSection(float lastY, int i){
        MethodInfo m = this.targetComponent.GetType().GetMethods()[i];

        string endString = m.Name + "(";
        foreach (ParameterInfo parm in m.GetParameters()){
            string type = parm.GetType().Name;
            endString = endString + " " + parm.Name+",";
        }  
        if(m.GetParameters().Length > 0)
            endString = endString.Remove(endString.Length - 1);
        endString = endString + " )";

        GUI.Label(new Rect(10f, lastY, 300f, 25f), endString);

        if (GUI.Button(new Rect(300f, lastY, 50f, 20f), "Invoke")){
            if(m.GetParameters().Length == 0)
                m.Invoke(this.targetComponent, null);
            else{
                this.targetMethod = m;
                this.currentState = 12;
            }
        }
    }

    private string parmInput = "Parameter Input (try to cast)";
    private void DrawInvokeWithParameters(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Invoke");

        string parms = string.Empty;
        foreach (ParameterInfo parm in this.targetMethod.GetParameters()){
            if (parms != string.Empty)
                parms = parms + ",";
            
            parms = parms + parm.ParameterType + " " + parm.Name;
        }
        
        GUI.Label(new Rect(10f, 30f, 500f, 100f), this.targetMethod.Name + "(" + parms + ")");

        this.parmInput = GUI.TextField(new Rect(10f, 50f, 250f, 50f), this.parmInput);

        if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back")){
            this.currentState = 11;
            return;
        }

        if (GUI.Button(new Rect(200f, 125f, 70f, 20f), "Invoke")){
            int i = 0;
            string[] args = this.parmInput.Split('#');
            object[] parameters = new object[this.targetMethod.GetParameters().Length];
            foreach (ParameterInfo p in this.targetMethod.GetParameters()){
                parameters[i] = TryToCastString(args[i], p.ParameterType);
                i++;
            }
            this.targetMethod.Invoke(this.targetComponent, parameters);
        }
    }

    public object TryToCastString(string valStr, Type type){
        object var = null;

        if (type.Equals(typeof(int)))
            var = (int)StringToFloat(valStr);
        else if (type.Equals(typeof(float)))
            var = StringToFloat(valStr);
        else if (type.Equals(typeof(UnityEngine.Vector2)))
            var = new Vector2(StringToFloat(valStr.Split(',')[0]), StringToFloat(valStr.Split(',')[1]));
        else if (type.Equals(typeof(UnityEngine.Vector3)))
            var = new Vector3(StringToFloat(valStr.Split(',')[0]), StringToFloat(valStr.Split(',')[1]), StringToFloat(valStr.Split(',')[2]));
        else if (type.Equals(typeof(UnityEngine.Vector4)))
            var = new Vector4(StringToFloat(valStr.Split(',')[0]), StringToFloat(valStr.Split(',')[1]), StringToFloat(valStr.Split(',')[2]), StringToFloat(valStr.Split(',')[3]));
        else if (type.Equals(typeof(bool)) | type.Equals(typeof(Boolean))){
            if (valStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                var = false;
            else
                var = true;
        }else if (type.Equals(typeof(string)) | type.Equals(typeof(String)))
            var = (string) valStr;
         else if (type.Equals(typeof(Int32))){
               var = valStr;
        Int32 output = 0;
            Int32.TryParse(valStr, out output);
            var = output;
        }else if (type.Equals(typeof(Int16))){
            Int16 output = 0;
            Int16.TryParse(valStr, out output);
            var = output;
        }else if (type.Equals(typeof(Int64))){
            Int64 output = 0;
            Int64.TryParse(valStr, out output);
            var = output;
        }else if (type.Equals(typeof(Color))){
            Color color = Color.white;
            ColorUtility.TryParseHtmlString("#" + valStr, out color);
            var = color;
        }

        return var;
    }

    private void DrawTargetCamera(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Target Camera");
        if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back"))
            currentState = 1;

        float lastY = 30;
        for (int i = 0; i < Camera.allCameras.Length; i++){
            try
            {
                GUI.Label(new Rect(10f, lastY, 300f, 30f), Camera.allCameras[i].name);

                if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Choose")){
                    targetCamera = Camera.allCameras[i];
                    targetObject = targetCamera.gameObject;
                    currentState = 1;
                }

                lastY = lastY + 25;
            }
            catch (Exception e){
                UnityEngine.Debug.LogWarning(e.Message);
            }
        }
    }

    private string timeInput = "1";
    private void DrawTimeScreen(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Time");
        GUI.Label(new Rect(10f, 30f, 200f, 50f), "Delta Time: " + Time.deltaTime+"\nTime Scale: "+Time.timeScale);

        timeInput = GUI.TextField(new Rect(10f, 65f, 70, 18), timeInput);
        if(GUI.Button(new Rect(10f, 85f, 70f, 20f), "Change"))
            Time.timeScale = StringToFloat(timeInput);
    }

    private string posXInput = "0";
    private string posYInput = "0";
    private string posZInput = "0";
    private string rotXInput = "0";
    private string rotYInput = "0";
    private string rotZInput = "0";
    private string scaXInput = "0";
    private string scaYInput = "0";
    private string scaZInput = "0";
    private void DrawChange(){
        GUI.Box(new Rect(0f, 0f, 500f, 150f), this.NAME + " - Change");

        if (targetObject != null){
            if (GUI.Button(new Rect(400f, 125f, 70f, 20f), "Back"))
                currentState = 1;

            GUI.Label(new Rect(10f, 30f, 100f, 50f), "Postion");
            if (GUI.Button(new Rect(60f, 33f, 60f, 15f), "Get")){
                posXInput = targetObject.transform.position.x.ToString();
                posYInput = targetObject.transform.position.y.ToString();
                posZInput = targetObject.transform.position.z.ToString();
            }

            if (GUI.Button(new Rect(130f, 33f, 60f, 15f), "Set")){
                targetObject.transform.position = new Vector3(StringToFloat(posXInput), StringToFloat(posYInput), StringToFloat(posZInput));
            }

            posXInput =  GUI.TextField(new Rect(10f, 55f, 70, 18), posXInput);
            posYInput = GUI.TextField(new Rect(85f, 55f, 70, 18), posYInput);
            posZInput = GUI.TextField(new Rect(160f, 55f, 70, 18), posZInput);

            GUI.Label(new Rect(10f, 80f, 100f, 50f), "Scale");
            if (GUI.Button(new Rect(60f, 83f, 60f, 15f), "Get")){
                scaXInput = targetObject.transform.localScale.x.ToString();
                scaYInput = targetObject.transform.localScale.y.ToString();
                scaZInput = targetObject.transform.localScale.z.ToString();
            }

            if (GUI.Button(new Rect(130f, 83f, 60f, 15f), "Set")){
                targetObject.transform.localScale = new Vector3(StringToFloat(scaXInput), StringToFloat(scaYInput), StringToFloat(scaZInput));
            }

            scaXInput = GUI.TextField(new Rect(10f, 105f, 70, 18), scaXInput);
            scaYInput = GUI.TextField(new Rect(85f, 105f, 70, 18), scaYInput);
            scaZInput = GUI.TextField(new Rect(160f, 105f, 70, 18), scaZInput);

            GUI.Label(new Rect(250f, 30f, 100f, 50f), "Rotation");
            if (GUI.Button(new Rect(310f, 33f, 60f, 15f), "Get")){
                rotXInput = targetObject.transform.eulerAngles.x.ToString();
                rotYInput = targetObject.transform.eulerAngles.y.ToString();
                rotZInput = targetObject.transform.eulerAngles.z.ToString();
            }

            if (GUI.Button(new Rect(380f, 33f, 60f, 15f), "Set")){
                targetObject.transform.rotation = Quaternion.Euler(StringToFloat(rotXInput), StringToFloat(rotYInput), StringToFloat(rotZInput));
            }

            rotXInput = GUI.TextField(new Rect(250f, 55f, 70, 18), rotXInput);
            rotYInput = GUI.TextField(new Rect(325f, 55f, 70, 18), rotYInput);
            rotZInput = GUI.TextField(new Rect(400f, 55f, 70, 18), rotZInput);
        }
        else
            currentState = 1;
    }

    private void DrawComponents(){
        GUI.Box(new Rect(0f, 0f, 450f, 150f), this.NAME + " - Components");

        DrawScroolSection(targetObject, 3, targetObject.GetComponents(typeof(UnityEngine.Component)).Length, 0, 4);
    }

    private void DrawComponentsSection(float lastY, int i){
        UnityEngine.Component comp = targetObject.GetComponents(typeof(UnityEngine.Component))[i];
        string compName = comp.ToString();
        compName = compName.Replace("UnityEngine.", "");
        GUI.Label(new Rect(10f, lastY, 300f, 30f), compName);
        Behaviour behaviour = (Behaviour)comp;

        string buttonName;
        if (behaviour.enabled)
            buttonName = "Disable";
        else
            buttonName = "Enable";

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), buttonName))
            behaviour.enabled = !behaviour.enabled;

        if (GUI.Button(new Rect(280f, lastY, 70f, 20f), "Values")){
            targetComponent = behaviour;
            currentState = 10;
        }

        if (GUI.Button(new Rect(360f, lastY, 70f, 20f), "Methodes")){
            targetComponent = behaviour;
            currentState = 11;
        }
    }


    private void DrawObjects(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Objects");
        if (GUI.Button(new Rect(10f, 2.5f, 100f, 20f), "Target Camera"))
            this.currentState = 6;

        if (this.targetObject != null){

            GUI.Label(new Rect(10f, 30f, 200f, 50f), "Target GameObject: " + this.targetObject.name + "\nTransform instance id: " + this.targetObject.transform.GetInstanceID());

            if (GUI.Button(new Rect(10f, 100f, 70f, 20f), "Destroy"))
                Destroy(this.targetObject);
            if (GUI.Button(new Rect(10f, 75f, 70f, 20f), "Copy"))
                Instantiate(this.targetObject);

            string swtichActiveString = string.Empty;
            if (this.targetObject.active)
                swtichActiveString = "Disable";
            else
                swtichActiveString = "Enable";
            if (GUI.Button(new Rect(10f, 125f, 70f, 20f), swtichActiveString))
                this.targetObject.SetActive(!this.targetObject.active);

            if (GUI.Button(new Rect(90f, 125f, 85f, 20f), "Components"))
                this.currentState = 3;

            if (GUI.Button(new Rect(180f, 125f, 55f, 20f), "Layer"))
                this.currentState = 15;

            if (this.targetObject.transform.parent != null){
                if (GUI.Button(new Rect(90f, 100f, 85f, 20f), "Parent"))
                    this.targetObject = targetObject.transform.parent.gameObject;
            }

            if(this.targetObject.transform.childCount > 0){
                if (GUI.Button(new Rect(90f, 75f, 85f, 20f), "Childrens"))
                    this.currentState = 5;
            }

            GUI.Label(new Rect(250f, 30f, 200f, 100f), "Position\n" + "X:" + this.targetObject.transform.position.x + " Y:" + this.targetObject.transform.position.y + " Z:" + this.targetObject.transform.position.z);
            GUI.Label(new Rect(250f, 60f, 200f, 100f), "Rotation\n" + "X:" + this.targetObject.transform.eulerAngles.x + " Y:" + this.targetObject.transform.eulerAngles.y + " Z:" + this.targetObject.transform.eulerAngles.z);
            GUI.Label(new Rect(250f, 90f, 200f, 100f), "Scale\n" + "X:" + this.targetObject.transform.localScale.x + " Y:" + this.targetObject.transform.localScale.y + " Z:" + this.targetObject.transform.localScale.z);

            if(GUI.Button(new Rect(250f, 125f, 70f, 20f), "Change"))
                this.currentState = 2;
        }
        else
            GUI.Label(new Rect(10f, 30f, 500f, 100f), "Please click on an GameObject and press \"i\"!");
    }

    public void DrawObjectLayer(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Object Layer");
        GUI.Label(new Rect(10f, 30, 500f, 100f), "Current Layer: " + LayerMask.LayerToName(this.targetObject.layer));
        DrawScroolSection(this.targetObject, 4, 31, 0, 5);
    }

    public void DrawObjectLayerSection(float lastY, int i){
        GUI.Label(new Rect(10f, lastY, 300f, 30f), LayerMask.LayerToName(i));

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Set"))
            this.targetObject.layer = i;
    }

    public void DrawTCPMenu(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - TCP");

        string connectionText = string.Empty;
        if (tcp.listener.Server.IsBound){
            if(tcp.client == null)
                connectionText = "Waiting for connection...";
            else{ 
                if (!tcp.client.Connected)
                    connectionText = "Waiting for connection...";
                else
                    connectionText = "Client is connected.";
            }

            if (GUI.Button(new Rect(10f, 100f, 70f, 20f), "Stop"))
                tcp.listener.Stop();
        }else{
            connectionText = "TCP-Server is not started.";
            if (GUI.Button(new Rect(10f, 100f, 70f, 20f), "Start"))
                tcp.StartServer();
        }

        GUI.Label(new Rect(10f, 30f, 200f, 50f), connectionText);
    }

    private int lastEventNumber;
    private int scroolSectionBeginValue = 0;
    private void DrawScroolSection(object nullCheck, int maxProSite, int lenght, int beginValue, int eventNumber, int backState = 1, bool noBackButton = false) {
        if (nullCheck != null){
            if (lastEventNumber != eventNumber)
                scroolSectionBeginValue = 0;
            lastEventNumber = eventNumber;

            if (!noBackButton){
                if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back")){
                    currentState = backState;
                    scroolSectionBeginValue = 0;
                }
            }

            float lastY = 30;
            lenght = lenght + 1;
            int unchangedLength = lenght;

            if (unchangedLength > maxProSite){
                lenght = maxProSite;
                if (scroolSectionBeginValue + 1 < unchangedLength - maxProSite){
                    if (GUI.Button(new Rect(10f, 125f, 70f, 20f), "↓"))
                        scroolSectionBeginValue++;
                }

                if (scroolSectionBeginValue != 0){
                    if (GUI.Button(new Rect(90f, 125f, 70f, 20f), "↑"))
                        scroolSectionBeginValue--;
                }
            }
            else
                scroolSectionBeginValue = 0;

            int z = 0;
            for (int i = scroolSectionBeginValue; z < lenght; i++){
                try{
                    if (i < unchangedLength){
                        lastY = lastY + 25;
                        z++;

                        if (eventNumber == 0)
                            DrawLoadSceneSection(lastY, i);
                        else if (eventNumber == 1)
                            DrawChildrensSection(lastY, i);
                        else if (eventNumber == 2)
                            DrawCompValuesSection(lastY, i);
                        else if (eventNumber == 3)
                            DrawCompMethodesSection(lastY, i);
                        else if (eventNumber == 4)
                            DrawComponentsSection(lastY, i);
                        else if (eventNumber == 5)
                            DrawObjectLayerSection(lastY, i);
                        else if (eventNumber == 6)
                            DrawConsoleSection(lastY, i);
                        else if (eventNumber == 7)
                            DrawResourcesSection(lastY, i);
                    }
                }
                catch (Exception e){
                    if(this.logExceptions)
                        UnityEngine.Debug.Log("MESSAGE: " + e.Message + "| STACKTRACE: " + e.StackTrace);
                }
            }
        }
    }

    public void AddComponentFromFile(string path){

        StreamReader readStm2 = new StreamReader(path);
        string fileIn2 = readStm2.ReadToEnd();
        readStm2.Close();

        string sourceCode = fileIn2;
       
    }

    private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type){
        string log = type + " " + logString + "\n" + stackTrace + "\n";
        if (!consoleLogs.Contains(log))
            consoleLogs.Add(log);

        PDebug2.tcp.SendClientMessage("CONSOLE: [" + type + "] " + logString + "| StackTrace: " + stackTrace);
    }

    public static float StringToFloat(string text){
        float output = 0;
        float.TryParse(text, out output);

        return output;
    }

    public string GetSavedName(){
        return this.name;
    }

    public GameObject GetTargetGameObject(){
        return this.targetObject;
    }

    public void SetTargetGameObject(GameObject obj){
        this.targetObject = obj;
    }

    public void SetCurrentState(int state){
        this.currentState = state;
    }
}

public class PDebugTCP : MonoBehaviour{/*Credits https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb for help at connection*/ 

    public string ip = "127.0.0.1";
    public Int32 port = 13000;

    public TcpClient client;
    public TcpListener listener;

    private Thread tcpListenerThread;

    private string message = null;

    public void StartServer(){
        Application.runInBackground = true;

        this.tcpListenerThread = new Thread(new ThreadStart(ServerListener));
        this.tcpListenerThread.IsBackground = true;
        this.tcpListenerThread.Start();
    }

    private void Update(){
        if (!String.IsNullOrEmpty(this.message)){/*Because much Functions are not possible in other Thread then the Main Thread*/
            PerformCommand(this.message);
            this.message = null;
        }
    }

    private void ServerListener(){
        IPAddress addr = IPAddress.Parse(this.ip);

        this.listener = new TcpListener(addr, this.port);
        this.listener.Start();

        UnityEngine.Debug.Log("Waiting for a connection...");
        this.client = null;
        Byte[] bytes = new Byte[1024];

        try { 
        while (true){
            using (this.client = this.listener.AcceptTcpClient()){
                UnityEngine.Debug.Log("Connected!");

                using (NetworkStream stream = this.client.GetStream()){
                    int lenght;

                    while((lenght = stream.Read(bytes, 0, bytes.Length)) != 0){
                        var data = new byte[lenght];
                        Array.Copy(bytes, 0, data, 0, lenght);
                        message = Encoding.ASCII.GetString(data);
                    }
                }
            }
        }}catch(Exception e){
            UnityEngine.Debug.Log("Client is disconnected, stopping Server...");
            this.listener.Stop();
        }
    }

    private void PerformCommand(String command){
        switch (command){

            case "getName":
                SendClientMessage(PDebug2.instance.GetSavedName());
                break;

            case "getGameName":
                SendClientMessage(Application.productName);
                break;

            case "getUnityVersion":
                SendClientMessage(Application.unityVersion);
                break;

            case "getTime":
                SendClientMessage("TimeScale: " + Time.timeScale + ", DeltaTime: " + Time.deltaTime);
            break;

            case "disable":
                PDebug2.instance.enabled = false;
                break;

            case "enable":
                PDebug2.instance.enabled = true;
                break;

            case "listSceneObjects":
                StartCoroutine(ListSceneObjects());
            break;

            case "getPosition":
                SendClientMessage(PDebug2.instance.GetTargetGameObject().transform.position.ToString());
            break;

            case "getRotation":
                SendClientMessage(PDebug2.instance.GetTargetGameObject().transform.rotation.eulerAngles.ToString());
            break;

            case "getScale":
                SendClientMessage(PDebug2.instance.GetTargetGameObject().transform.localScale.ToString());
            break;

            case "getInstanceID":
                SendClientMessage(PDebug2.instance.GetTargetGameObject().transform.GetInstanceID().ToString());
            break;

            case "enableObject":
                PDebug2.instance.GetTargetGameObject().SetActive(true);
            break;

            case "disableObject":
                PDebug2.instance.GetTargetGameObject().SetActive(false);
            break;

            case "destroyObject":
                Destroy(PDebug2.instance.GetTargetGameObject());
            break;

            case "dontDestroyObjectAtLoad":
                DontDestroyOnLoad(PDebug2.instance.GetTargetGameObject());
            break;

            case "clear":
                UnityEngine.Debug.ClearDeveloperConsole(); 
            break;

            case "quit":
                Application.Quit();
            break;

            case "compareScene":
                StartCoroutine(CompareScene());
            break;

            case "fullscreen":
                if (Screen.fullScreen)
                    Screen.fullScreen = false;
                else
                    Screen.fullScreen = true;
            break;

            default:
                string[] args = command.Split(' ');

                if (command.StartsWith("setTargetObject")){
                    PDebug2.instance.SetTargetGameObject(GameObject.Find(ConnectArgs(args, 1)));
                }else if (command.StartsWith("setPosition")){
                    float x = PDebug2.StringToFloat(args[1]);
                    float y = PDebug2.StringToFloat(args[2]);
                    float z = PDebug2.StringToFloat(args[3]);

                    PDebug2.instance.GetTargetGameObject().transform.position = new Vector3(x, y, z);
                }else if (command.StartsWith("setRotation")){
                    float x = PDebug2.StringToFloat(args[1]);
                    float y = PDebug2.StringToFloat(args[2]);
                    float z = PDebug2.StringToFloat(args[3]);

                    PDebug2.instance.GetTargetGameObject().transform.rotation = Quaternion.Euler(x, y, z);
                }else if (command.StartsWith("setScale")){
                    float x = PDebug2.StringToFloat(args[1]);
                    float y = PDebug2.StringToFloat(args[2]);
                    float z = PDebug2.StringToFloat(args[3]);

                    PDebug2.instance.GetTargetGameObject().transform.localScale = new Vector3(x, y, z);
                }else if (command.StartsWith("translate")){
                    float x = PDebug2.StringToFloat(args[1]);
                    float y = PDebug2.StringToFloat(args[2]);
                    float z = PDebug2.StringToFloat(args[3]);

                    PDebug2.instance.GetTargetGameObject().transform.Translate(x, y, z);
                }else if (command.StartsWith("setTime")){
                    float time = PDebug2.StringToFloat(args[1]);
                    Time.timeScale = time;
                }else if (command.StartsWith("saveSceneData")){
                    int numb = (int) PDebug2.StringToFloat(args[1]);

                    if (numb == 0)
                        savedSceneObjects0 = SaveScene();
                    else
                        savedSceneObjects1 = SaveScene();
                }else if (command.StartsWith("loadSceneData")){
                    int numb = (int)PDebug2.StringToFloat(args[1]);

                    if (numb == 0)
                        LoadSceneData(savedSceneObjects0);
                    else
                        LoadSceneData(savedSceneObjects1);
                }else if (command.StartsWith("addScriptFromPath")){
                    PDebug2.instance.AddComponentFromFile(ConnectArgs(args, 1));
                }else if (command.StartsWith("setCurrentState")){
                    int numb = (int)PDebug2.StringToFloat(args[1]);

                    PDebug2.instance.SetCurrentState(numb);
                }else if (command.StartsWith("setMaxUsedMemory")){
                    int numb = (int)PDebug2.StringToFloat(args[1]);

                }
                break;
        }
    }

    private List<SaveSceneObject> savedSceneObjects0;
    private List<SaveSceneObject> savedSceneObjects1;

    [System.Serializable]
    private class SaveSceneObject{
        public GameObject obj;
        public string name;
        public int instanceID;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool hasChecked = false;

        public SaveSceneObject(GameObject obj){
            this.obj = obj;
            this.name = obj.name;
            this.instanceID = obj.GetInstanceID();
            this.position = obj.transform.localPosition;
            this.rotation = obj.transform.localEulerAngles;
            this.scale = obj.transform.localScale;
            this.hasChecked = false;
        }
    }

    private void LoadSceneData(List<SaveSceneObject> savs){
        foreach (SaveSceneObject sav in savs){
            if(sav.obj != null){
                sav.obj.transform.localPosition = sav.position;
                sav.obj.transform.localEulerAngles = sav.rotation;
                sav.obj.transform.localScale = sav.scale;
            }
        }
    }

    private IEnumerator CompareScene(){
        if (savedSceneObjects0 == null | savedSceneObjects1 == null){
            SendClientMessage("Can't compare Scene, plase save the Scene using saveSceneData 0 and saveSceneData 1.");
            SendClientMessage("endSceneList123");
            yield return null;
        }else{ 

            foreach (SaveSceneObject item in savedSceneObjects0){
                item.hasChecked = false;
            }

            foreach (SaveSceneObject item in savedSceneObjects1){
                item.hasChecked = false;
            }

            foreach (SaveSceneObject sav0 in savedSceneObjects0){
                SaveSceneObject sav1 = GetSavFromInstanceID(sav0);
                if (sav1 != null){
                     if (sav0.position != sav1.position)
                         SendClientMessage(sav0.name + " changed Position " + "0: " + sav0.position + ", 1:" + sav1.position + "\n");
                     if (sav0.rotation != sav1.rotation)
                         SendClientMessage(sav0.name + " changed Rotation " + "0: " + sav0.rotation + ", 1:" + sav1.rotation + "\n");
                     if (sav0.scale != sav1.scale)
                         SendClientMessage(sav0.name + " changed Scale " + "0: " + sav0.scale + ", 1:" + sav1.scale + "\n");

                     sav0.hasChecked = true;
                     sav1.hasChecked = true;
                }else
                     SendClientMessage(sav0.name + " exited in 0 but destroyed in 1!\n");
            }

            foreach (SaveSceneObject item in savedSceneObjects1){
                if (!item.hasChecked)
                    SendClientMessage(item.name + " was created in 1!\n");
            }
        }

        yield return new WaitForSeconds(0.1f);
        SendClientMessage("endSceneList123");
    }

    private SaveSceneObject GetSavFromInstanceID(SaveSceneObject sav){
        foreach (SaveSceneObject item in savedSceneObjects1){
            if (sav.instanceID == item.instanceID)
                return item;
        }

        return null;
    }

    private List<SaveSceneObject> SaveScene(){
        List<SaveSceneObject> savedSceneObjects = new List<SaveSceneObject>();

        savedSceneObjects.Clear();

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects){
            savedSceneObjects.Add(new SaveSceneObject(obj));
        }

        return savedSceneObjects;
    }


    private IEnumerator ListSceneObjects(){
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        List<string> list = new List<string>();
   
        foreach (GameObject obj in allObjects){
            list.Add(obj.name);
        }

        list.Sort();

        foreach(string oname in list){
            string n = oname;
            if (!oname.Contains("\n"))
                n = oname + "\n";

            SendClientMessage(n);
        }

        yield return new WaitForSeconds(1);

        SendClientMessage("endSceneList123");
    }

    public void SendClientMessage(String message){
        if (this.client == null)
            return;

        try{
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite){
                byte[] messageAsByteArray = Encoding.ASCII.GetBytes(message);
                stream.Write(messageAsByteArray, 0, messageAsByteArray.Length);
            }
        }catch (SocketException e){

        }
    }

    public string ConnectArgs(string[] args, int beginValue){
        string message = string.Empty;

        for (int i = beginValue; i < args.Length; i++){
            args[i] = args[i] + " ";
        }

        args[args.Length-1] = args[args.Length-1].Substring(0, args[args.Length-1].Length-1);

        for (int i = beginValue; i < args.Length; i++){
            message = message + args[i];
        }
        return message;
    }

}

public class FreeCamController : MonoBehaviour {

    public float moveSpeed = 8;
    public float fastMoveSpeed = 25;
    public float sensitivity = 3;

    private bool canMove = true;

    private void Update(){
        if (Input.GetKeyDown(KeyCode.I))
            this.canMove = !this.canMove;

        if (Input.GetKey(KeyCode.U))
            transform.Rotate(Vector3.forward * 20 * Time.deltaTime);
        else if (Input.GetKey(KeyCode.J))
            transform.Rotate(Vector3.back * 20 * Time.deltaTime);
        if (Input.GetKey(KeyCode.K))
            transform.Rotate(Vector3.right * 20 * Time.deltaTime);
        else if (Input.GetKey(KeyCode.J))
            transform.Rotate(Vector3.left * 20 * Time.deltaTime);

        if (!this.canMove)
            return;

        float speed = this.moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed = this.fastMoveSpeed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            transform.position = transform.position + (-transform.right * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            transform.position = transform.position + (transform.right * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            transform.position = transform.position + (transform.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            transform.position = transform.position + (-transform.forward * speed * Time.deltaTime);

        float newRotationX = transform.localEulerAngles.y + Input.GetAxisRaw("Mouse X") * sensitivity;
        float newRotationY = transform.localEulerAngles.x - Input.GetAxisRaw("Mouse Y") * sensitivity;
        transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
    }

}