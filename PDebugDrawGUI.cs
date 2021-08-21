using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PDebugDrawGUI : MonoBehaviour{

    public static PDebugDrawGUI instance = null;

    private int currentState = 0;/*0 = Console , 1 = Objects , 2 = Change , 3 = Components , 4 = Time , 5 = Childrens , 6 = TargetCamera*/
    private string consoleString = string.Empty;/*Console Log*/
    private GameObject targetObject = null;/*Current GameObject in Objects Tab*/
    private Camera targetCamera = null;/*Camera that calculating the Ray*/

    private bool is3D = false;/*This Script use a other Ray for 3D Games!*/

    private void Awake(){
        targetCamera = Camera.main;

        /*Check if an instance allready exits and if exits destroy this else create a new gameobject with the instance*/
        if(instance == null){      
            if(gameObject.name != "PDEBUG-INSTANCE"){
                GameObject gm = new GameObject("PDEBUG-INSTANCE");
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
        }
    }

    private void DrawMainButtons(){
        if (GUI.Button(new Rect(10f, 155f, 70f, 20f), "Console"))
            currentState = 0;
        if (GUI.Button(new Rect(85f, 155f, 70f, 20f), "Objects"))
            currentState = 1;
        if (GUI.Button(new Rect(160f, 155f, 70f, 20f), "Time"))
            currentState = 4;
    }

    private void DrawConsole(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Console");
        GUI.TextField(new Rect(50f, 30f, 300f, 100f), consoleString);

        if (GUI.Button(new Rect(50, 128f, 70f, 20f), "Clear"))
            consoleString = string.Empty;
    }

    private void DrawChildrens(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Childrens");

        if (targetObject != null){
            if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back"))
                currentState = 1;

            float lastY = 30;
            for (int i = 0; i < targetObject.transform.childCount; i++) {
                try{
                    GUI.Label(new Rect(10f, lastY, 300f, 30f), targetObject.transform.GetChild(i).name);

                    if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Edit")){
                        targetObject = targetObject.transform.GetChild(i).gameObject;
                        currentState = 1;
                    }

                    lastY = lastY + 25;
                }
                catch (Exception e){
                    Debug.LogWarning(e.Message);
                }
            }
        }
    }

    private void DrawTargetCamera(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Target Camera");
        if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back"))
            currentState = 1;

        float lastY = 30;
        for (int i = 0; i < Camera.allCameras.Length; i++){
            try
            {
                GUI.Label(new Rect(10f, lastY, 300f, 30f), Camera.allCameras[i].name);

                if (GUI.Button(new Rect(200f, lastY, 70f, 20f), "Choose")){
                    targetCamera = Camera.allCameras[i];
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
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Time");
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
        GUI.Box(new Rect(0f, 0f, 500f, 150f), "PDEBUG - Change");

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
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Components");

        if (targetObject != null){
            if (GUI.Button(new Rect(300f, 125f, 70f, 20f), "Back"))
                currentState = 1;

            float lastY = 30;
            for (int i = 0; i < targetObject.GetComponents(typeof(Component)).Length; i++){
                try{
                    Component comp = targetObject.GetComponents(typeof(Component))[i];
                    Behaviour behaviour = (Behaviour)comp;
                    string compName = comp.ToString();
                    compName = compName.Replace("UnityEngine.", "");

                    GUI.Label(new Rect(10f, lastY, 300f, 30f), compName);

                    string buttonName;
                    if (behaviour.enabled)
                        buttonName = "Disable";
                    else
                        buttonName = "Enable"; 

                    if (GUI.Button(new Rect(200f, lastY, 70f, 20f), buttonName)){
                        behaviour.enabled = !behaviour.enabled;
                    }

                    lastY = lastY + 25;
                }catch(Exception e){
                    Debug.LogWarning(e.Message);
                }
            }
        }
    }

    private void DrawObjects(){
        GUI.Box(new Rect(0f, 0f, 400f, 150f), "PDEBUG - Objects");
        if (GUI.Button(new Rect(10f, 2f, 100f, 20f), "Target Camera"))
            currentState = 6;

        if (targetObject != null){

            GUI.Label(new Rect(10f, 30f, 200f, 50f), "Target GameObject: " + targetObject.name + "\nTransform instance id: " + targetObject.transform.GetInstanceID());

            if (GUI.Button(new Rect(10f, 100f, 70f, 20f), "Destroy"))
                Destroy(targetObject);

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
                if (GUI.Button(new Rect(178f, 125f, 70f, 20f), "Childrens"))
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

    private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type){
        consoleString = consoleString + type+" " + logString + "\n" + stackTrace+"\n";
    }

    private float StringToFloat(string text){
        float output = 0;
        float.TryParse(text, out output);

        return output;
    }
}
