using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ChatQueryManager : MonoBehaviour
{

    [System.Serializable]
    struct Message
    {
        public string type;
        public GameObject messagePrefab;
    }

    public static ChatQueryManager instance;

    // The URL of the webserver endpoint
    [SerializeField]
    private string url = "http://localhost:8181/chat/";
    public string username = "rover";
    private string fullUrl { get { return url + username; } }
    [SerializeField]
    private static string postUrl = "http://localhost:8181/chat/";
    public static string postUsername = "user1";
    private static string fullPostUrl { get { return postUrl + postUsername + "/" + instance.username + "?type=text"; } }

    // The frequency of the queries in seconds
    [SerializeField]
    private const float QUERY_SECONDS = 1f;

    [SerializeField]
    private GameObject messagesParentObejct;
    [SerializeField]
    private Message[] messageTypes;

    private string lastMessage = "";


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There should only be one ChatQueryManager in the scene");
        }
        StartCoroutine(QueryRoutine());
    }

    private IEnumerator QueryRoutine()
    {
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fullUrl))
            {
                // Send the request and wait for the results
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    ParseJson(webRequest.downloadHandler.text);                        
                }
            }
            yield return new WaitForSeconds(QUERY_SECONDS);
        }
    }

    private void ParseJson(string jsonString)
    {
        if (jsonString == lastMessage)
        {
            return;
        }
        //delete all childrern of the parent object
        foreach (Transform child in messagesParentObejct.transform)
        {
            Destroy(child.gameObject);
        }
        ServerResponse response = JsonUtility.FromJson<ServerResponse>(jsonString);
        foreach (var message in response.messages)
        {
            foreach (var messageType in messageTypes)
            {
                if (message.type == messageType.type)
                {
                    GameObject messageObject = Instantiate(messageType.messagePrefab, messagesParentObejct.transform);
                    messageObject.GetComponent<IMessageSetter>().SetMessage(message.content);
                }
            }
        }
        lastMessage = jsonString;
    }
    public static void SendChoiceResponse(string jsonString)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        UnityWebRequest www = new UnityWebRequest(fullPostUrl, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
    }
}

[System.Serializable]
public class ServerResponse
{
    public string name;
    public MessageContent[] messages;
}

[System.Serializable]
public class MessageContent
{
    public string type;
    public string content;
}