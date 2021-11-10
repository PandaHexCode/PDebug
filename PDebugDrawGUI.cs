using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PDebugDrawGUI : MonoBehaviour{

    public static PDebugDrawGUI instance = null;

    private string NAME = "PDebug";
    private int currentState = 0;/*0 = Console , 1 = Objects , 2 = Change , 3 = Components , 4 = Time , 5 = Childrens , 6 = TargetCamera , 7 = Scene , 8 = LoadScene*/
    private string consoleString = string.Empty;/*Console Log*/
    private GameObject targetObject = null;/*Current GameObject in Objects Tab*/
    private Camera targetCamera = null;/*Camera that calculating the Ray*/

    private bool is3D = false;/*This Script use a other Ray for 3D Games!*/
    private bool logExceptions = false;/*True needs lot more Performance!*/

    private void Awake(){
        targetCamera = Camera.main;
  
        /*Check if an instance allready exits and if exits destroy this else create a new gameobject with the instance*/
        if (instance == null){      
            if(gameObject.name != this.NAME+"-INSTANCE"){
                GameObject gm = new GameObject(this.NAME+"-INSTANCE");
                instance = gm.AddComponent<PDebugDrawGUI>();
                DontDestroyOnLoad(instance);
                Destroy(this);
            }
        }
        else if(instance != this)
            Destroy(this);

        Application.logMessageReceived += HandleLog;/*Register Console Log*/
    }

    private void Update(){
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if(currentState == 1){
            if (Input.GetKeyDown(KeyCode.I)){/*Try to get a TargetObject for the Objects Tab*/
                Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
                if (!is3D){/*2D*/
                    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                    if (hit.collider != null)
                        targetObject = hit.collider.gameObject;
                }
                else{/*3D*/         
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit)){
                        if (hit.collider != null)
                            targetObject = hit.collider.gameObject;
                    }
                }
            }
        }
    }

    private void OnGUI(){
        DrawMainButtons();

        switch (currentState){
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
        }
    }

    private void DrawMainButtons(){
        if (GUI.Button(new Rect(5f, 155f, 70f, 20f), "Console"))
            currentState = 0;
        if (GUI.Button(new Rect(80f, 155f, 70f, 20f), "Objects"))
            currentState = 1;
        if (GUI.Button(new Rect(155f, 155f, 70f, 20f), "Time"))
            currentState = 4;
        if (GUI.Button(new Rect(230f, 155f, 70f, 20f), "Scene"))
            currentState = 7;
        if (GUI.Button(new Rect(305f, 155f, 70f, 20f), "Applicat-"))
            currentState = 9;
    }

    private void DrawConsole(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME+" - Console");
        GUI.TextField(new Rect(50f, 30f, 300f, 100f), consoleString);

        if (GUI.Button(new Rect(50, 128f, 70f, 20f), "Clear"))
            consoleString = string.Empty;
    }

    private void DrawScene(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Scene");
        GUI.Label(new Rect(10f, 30f, 200f, 50f), "ActiveScene: "+SceneManager.GetActiveScene().name);
        if (GUI.Button(new Rect(10f, 70f, 85f, 20f), "Load Scene"))
            currentState = 8;
    }

    private int loadSceneBeginValue = 0;
    private void DrawLoadScene(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Load Scene");

        DrawScroolSection(1, 3, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings, 0, 0);
    }

    private void LoadSceneSection(float lastY, int i){
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

        DrawScroolSection(targetObject, 3, targetObject.transform.childCount, 0, 1);
    }

    private void DrawChildrensSection(float lastY, int i){
        GUI.Label(new Rect(10f, lastY, 300f, 30f), targetObject.transform.GetChild(i).name);

        if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Edit")){
            currentState = 1;
            targetObject = targetObject.transform.GetChild(i).gameObject;
            return;
        }
    }

    private Component targetComponent = null;

    private void DrawCompValues(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Comp Values");

        DrawScroolSection(targetComponent, 3, targetComponent.GetType().GetProperties().Length, 0, 2, 3);
    }

    private void DrawCompValuesSection(float lastY, int i){
        PropertyInfo prop = targetComponent.GetType().GetProperties()[i];
        if (prop.Name.Equals("rigidbody") | prop.Name.Equals("rigidbody2D") | prop.Name.Equals("camera"))
            return;
        else
        {
            var value = prop.GetValue(targetComponent, null);

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

        DrawScroolSection(targetComponent, 3, targetComponent.GetType().GetMethods().Length, 0, 3, 3);
    }

    private void DrawCompMethodesSection(float lastY, int i){
        MethodInfo m = targetComponent.GetType().GetMethods()[i];

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
                m.Invoke(targetComponent, null);
            else{
                this.method = m;
                this.currentState = 12;
            }
        }
    }

    private MethodInfo method;
    private bool wasInitInvoke = false;
    private void DrawInvokeWithParameters(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), this.NAME + " - Invoke");
        if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back")){
            tempParms.Clear();
            currentState = 11;
            wasInitInvoke = false;
            return;
        }

        if (GUI.Button(new Rect(200f, 125f, 70f, 20f), "Invoke"))
            method.Invoke(targetComponent, tempParms.ToArray());

        if (!wasInitInvoke){
            scroolSectionBeginValue = 0;
            tempParms.Clear();
            wasInitInvoke = true;

            foreach (ParameterInfo parm in method.GetParameters()){
                if (parm.HasDefaultValue)/*Some System.Reflection Versions don't have this Method(parm.HasDefualtValue), then simple remove this*/
                    tempParms.Add(parm.DefaultValue);
                else{
                    if (parm.ParameterType.Equals(typeof(bool)))
                        tempParms.Add(true);
                    else if (parm.ParameterType.Equals(typeof(int)))
                        tempParms.Add(1);
                    else if (parm.ParameterType.Equals(typeof(float)))
                        tempParms.Add(0.1f);
                    else if (parm.ParameterType.Equals(typeof(Vector2)))
                        tempParms.Add(new Vector2(0, 0));
                    else if (parm.ParameterType.Equals(typeof(Vector3)))
                        tempParms.Add(new Vector3(0, 0, 0));
                    else{
                        currentState = 11;
                        wasInitInvoke = false;
                        Debug.LogWarning("Sorry but " + this.NAME + " can't edit this value type(" + parm.ParameterType + ")yet!");
                        return;
                    } 
                }
            }
        }

        DrawScroolSection(targetComponent, 3, method.GetParameters().Length, 0, 5);
    }

    private List<object> tempParms = new List<object>(10);
    private void DrawInvokeSection(float lastY, int i){
        ParameterInfo parm = method.GetParameters()[i];
        object value = tempParms[i];

        GUI.Label(new Rect(10f, lastY, 300f, 25f), parm.Name + " - " + value);

        if (value.GetType().Equals(typeof(bool))){
            if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Change"))
                tempParms[i] = !(bool)value;
        }
        else if (value.GetType().Equals(typeof(int))){
            if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "+"))
                tempParms[i] = (int)value + 1;
            if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "-"))
                tempParms[i] = (int)value - 1;
        }
        else if (value.GetType().Equals(typeof(float))){
            if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "+"))
                tempParms[i] = (float)value + 0.1f;
            if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "-"))
                tempParms[i] = (float)value - 0.1f;
        }else if (value.GetType().Equals(typeof(Vector2))){
            if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "x+"))
                tempParms[i] = (Vector2)value + new Vector2(0.1f, 0);
            if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "x-"))
                tempParms[i] = (Vector2)value + new Vector2(-0.1f, 0);
            if (GUI.Button(new Rect(270f, lastY, 30f, 20f), "y+"))
                tempParms[i] = (Vector2)value + new Vector2(0, 0.1f);
            if (GUI.Button(new Rect(305f, lastY, 30f, 20f), "y-"))
                tempParms[i] = (Vector2)value + new Vector2(0, -0.1f);
        }else if (value.GetType().Equals(typeof(Vector3))){
            if (GUI.Button(new Rect(200f, lastY, 30f, 20f), "x+"))
                tempParms[i] = (Vector3)value + new Vector3(0.1f, 0 ,0);
            if (GUI.Button(new Rect(235f, lastY, 30f, 20f), "x-"))
                tempParms[i] = (Vector3)value + new Vector3(-0.1f, 0, 0);
            if (GUI.Button(new Rect(270f, lastY, 30f, 20f), "y+"))
                tempParms[i] = (Vector3)value + new Vector3(0, 0.1f, 0);
            if (GUI.Button(new Rect(305f, lastY, 30f, 20f), "y-"))
                tempParms[i] = (Vector3)value + new Vector3(0, -0.1f, 0);
            if (GUI.Button(new Rect(340f, lastY, 30f, 20f), "z+"))
                tempParms[i] = (Vector3)value + new Vector3(0, 0, 0.1f);
            if (GUI.Button(new Rect(375f, lastY, 30f, 20f), "z-"))
                tempParms[i] = (Vector3)value + new Vector3(0, 0, -0.1f);
        }
        else{
            currentState = 11;
            wasInitInvoke = false;
            Debug.LogWarning("Sorry but " + this.NAME + " can't edit this value type(" + tempParms[i].GetType() + ")yet!");
        }
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
                Debug.LogWarning(e.Message);
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

        DrawScroolSection(targetObject, 3, targetObject.GetComponents(typeof(Component)).Length, 0, 4);
    }

    private void DrawComponentsSection(float lastY, int i){
        Component comp = targetObject.GetComponents(typeof(Component))[i];
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
            currentState = 6;

        if (targetObject != null){

            GUI.Label(new Rect(10f, 30f, 200f, 50f), "Target GameObject: " + targetObject.name + "\nTransform instance id: " + targetObject.transform.GetInstanceID());

            if (GUI.Button(new Rect(10f, 100f, 70f, 20f), "Destroy"))
                Destroy(targetObject);
            if (GUI.Button(new Rect(10f, 75f, 70f, 20f), "Copy"))
                Instantiate(targetObject);

            string swtichActiveString = string.Empty;
            if (targetObject.active)
                swtichActiveString = "Disable";
            else
                swtichActiveString = "Enable";
            if (GUI.Button(new Rect(10f, 125f, 70f, 20f), swtichActiveString))
                targetObject.SetActive(!targetObject.active);

            if (GUI.Button(new Rect(90f, 125f, 85f, 20f), "Components"))
                currentState = 3;

            if (targetObject.transform.parent != null){
                if (GUI.Button(new Rect(90f, 100f, 85f, 20f), "Parent"))
                    targetObject = targetObject.transform.parent.gameObject;
            }

            if(targetObject.transform.childCount > 0){
                if (GUI.Button(new Rect(90f, 75f, 85f, 20f), "Childrens"))
                    currentState = 5;
            }

            GUI.Label(new Rect(250f, 30f, 200f, 100f), "Position\n" + "X:" + targetObject.transform.position.x + " Y:" + targetObject.transform.position.y + " Z:" + targetObject.transform.position.z);
            GUI.Label(new Rect(250f, 60f, 200f, 100f), "Rotation\n" + "X:" + targetObject.transform.eulerAngles.x + " Y:" + targetObject.transform.eulerAngles.y + " Z:" + targetObject.transform.eulerAngles.z);
            GUI.Label(new Rect(250f, 90f, 200f, 100f), "Scale\n" + "X:" + targetObject.transform.localScale.x + " Y:" + targetObject.transform.localScale.y + " Z:" + targetObject.transform.localScale.z);

            if(GUI.Button(new Rect(250f, 125f, 70f, 20f), "Change"))
                currentState = 2;
        }
        else
            GUI.Label(new Rect(10f, 30f, 500f, 100f), "Please click on an GameObject and press \"i\"!");
    }

    private int lastEventNumber;
    private int scroolSectionBeginValue = 0;
    private void DrawScroolSection(object nullCheck, int maxProSite, int lenght, int beginValue, int eventNumber, int backState = 1) {
        if (nullCheck != null){
            if (lastEventNumber != eventNumber)
                scroolSectionBeginValue = 0;
            lastEventNumber = eventNumber;

            if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back")){
                currentState = backState;
                scroolSectionBeginValue = 0;
            }

            float lastY = 30;
            int unchangedLength = lenght;

            if (unchangedLength > maxProSite){
                lenght = maxProSite;
                if (scroolSectionBeginValue + 1 < unchangedLength - 3){
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

                        if(eventNumber == 0)
                                LoadSceneSection(lastY, i);
                        else if(eventNumber == 1)
                                DrawChildrensSection(lastY, i);
                        else if (eventNumber == 2)
                            DrawCompValuesSection(lastY, i);
                        else if (eventNumber == 3)
                            DrawCompMethodesSection(lastY, i);
                        else if (eventNumber == 4)
                            DrawComponentsSection(lastY, i);
                        else if (eventNumber == 5)
                            DrawInvokeSection(lastY, i);
                    }
                }
                catch (Exception e){
                    if(this.logExceptions)
                        Debug.Log("MESSAGE: " + e.Message + "| STACKTRACE: " + e.StackTrace);
                }
            }
        }
    }

    private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type){
        consoleString = consoleString + type+" " + logString + "\n" + stackTrace+"\n";
    }

    private float StringToFloat(string text){
        float output = 0;
        float.TryParse(text, out output);

        return output;
    }
}
