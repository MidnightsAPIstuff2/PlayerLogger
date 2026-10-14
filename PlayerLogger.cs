using System.Collections;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;

// made by shek

public class PlayerLogger : MonoBehaviourPunCallbacks
{
    [SerializeField] string webhookUrl = "";
    [SerializeField] float nameSyncWait = 2f;
    [SerializeField] float sendGap = 1.1f;

    Dictionary<string, string> seen = new Dictionary<string, string>();
    HashSet<string> banTodo = new HashSet<string>();
    Queue<string> outbox = new Queue<string>();
    bool pumping;

    public override void OnJoinedRoom()
    {
        seen.Clear();
        banTodo.Clear();

        string room = PhotonNetwork.CurrentRoom.Name;
        var sb = new StringBuilder();
        sb.AppendLine("room: " + room + " (" + PhotonNetwork.CurrentRoom.PlayerCount + " players)");

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            Record(p, sb);
        }

        Queue(sb.ToString());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartCoroutine(CheckLater(newPlayer));
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changed)
    {
        string id = IdOf(target);
        if (seen.ContainsKey(id) && seen[id] != target.NickName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("name change in " + PhotonNetwork.CurrentRoom.Name);
            Record(target, sb);
            Queue(sb.ToString());
        }
    }

    IEnumerator CheckLater(Player p)
    {
        yield return new WaitForSeconds(nameSyncWait);

        if (!PhotonNetwork.InRoom || p == null || p.IsInactive) yield break;

        var sb = new StringBuilder();
        sb.AppendLine("joined " + PhotonNetwork.CurrentRoom.Name);
        if (Record(p, sb)) Queue(sb.ToString());
    }

    bool Record(Player p, StringBuilder sb)
    {
        string id = IdOf(p);
        string name = p.NickName;

        if (seen.TryGetValue(id, out string old) && old == name) return false;
        seen[id] = name;

        bool noName = string.IsNullOrWhiteSpace(name);
        sb.AppendLine(id + "  |  " + (noName ? "<no name>" : name));

        if (noName && banTodo.Add(id))
        {
            sb.AppendLine("   ^ BAN THIS PERSON (THEY HAVE NO NAME)");
        }
        return true;
    }

    string IdOf(Player p)
    {
        return string.IsNullOrEmpty(p.UserId) ? "actor" + p.ActorNumber : p.UserId;
    }

    void Queue(string text)
    {
        outbox.Enqueue(text);
        if (!pumping) StartCoroutine(Pump());
    }

    IEnumerator Pump()
    {
        pumping = true;
        while (outbox.Count > 0)
        {
            yield return Post(outbox.Dequeue());
            yield return new WaitForSeconds(sendGap);
        }
        pumping = false;
    }

    IEnumerator Post(string text)
    {
        if (string.IsNullOrEmpty(webhookUrl)) yield break;

        if (text.Length > 1900) text = text.Substring(0, 1900) + "...";
        string json = "{\"content\":\"```\\n" + Escape(text) + "```\"}";

        using (var req = new UnityWebRequest(webhookUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[PlayerLogger] webhook failed: " + req.error);
            }
        }
    }

    static string Escape(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': break;
                case '\t': sb.Append("\\t"); break;
                case '`': sb.Append("'"); break;
                default:
                    if (c < 32) break;
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }
}
