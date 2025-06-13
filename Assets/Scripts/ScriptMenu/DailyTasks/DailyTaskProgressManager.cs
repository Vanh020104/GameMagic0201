
using System.Collections.Generic;
using UnityEngine;

public class DailyTaskProgressManager : MonoBehaviour
{
    public static DailyTaskProgressManager Instance;

    private Dictionary<string, int> progressMap = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ giữ lại khi load scene mới
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetProgress(string taskId)
    {
        return progressMap.ContainsKey(taskId) ? progressMap[taskId] : 0;
    }

    public void AddProgress(string taskId, int amount = 1)
    {
        if (!progressMap.ContainsKey(taskId))
            progressMap[taskId] = 0;

        progressMap[taskId] += amount;
    }
    public void ResetAllProgress()
    {
        progressMap.Clear(); 
    }

}



// using System.Collections.Generic;
// using UnityEngine;

// public class DailyTaskProgressManager : MonoBehaviour
// {
//     public static DailyTaskProgressManager Instance;

//     private Dictionary<string, int> progressMap = new();
//     private const string ProgressKey = "DailyTaskProgress";

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); // ✅ giữ lại khi load scene
//             LoadProgress();                // ✅ tự động load khi khởi động
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     /// <summary>
//     /// Lấy tiến độ nhiệm vụ theo ID
//     /// </summary>
//     public int GetProgress(string taskId)
//     {
//         return progressMap.TryGetValue(taskId, out int value) ? value : 0;
//     }

//     /// <summary>
//     /// Cộng tiến độ nhiệm vụ
//     /// </summary>
//     public void AddProgress(string taskId, int amount = 1)
//     {
//         if (!progressMap.ContainsKey(taskId))
//             progressMap[taskId] = 0;

//         progressMap[taskId] += amount;

//         SaveProgress(); // ✅ Lưu ngay mỗi lần thay đổi
//     }

//     /// <summary>
//     /// Reset toàn bộ tiến trình khi sang ngày mới
//     /// </summary>
//     public void ResetAllProgress()
//     {
//         progressMap.Clear();
//         SaveProgress(); // ✅ reset thì cũng lưu luôn
//     }

//     /// <summary>
//     /// Gọi khi thoát ứng dụng (editor hoặc mobile)
//     /// </summary>
//     private void OnApplicationQuit()
//     {
//         SaveProgress();
//     }

//     /// <summary>
//     /// Lưu tiến độ hiện tại vào PlayerPrefs
//     /// </summary>
//     private void SaveProgress()
//     {
//         List<string> entries = new();

//         foreach (var kvp in progressMap)
//         {
//             // Bỏ qua những task có giá trị = 0 (không cần lưu)
//             if (kvp.Value > 0)
//                 entries.Add($"{kvp.Key}:{kvp.Value}");
//         }

//         PlayerPrefs.SetString(ProgressKey, string.Join(",", entries));
//         PlayerPrefs.Save();
//     }

//     /// <summary>
//     /// Tải lại tiến độ từ PlayerPrefs
//     /// </summary>
//     public void LoadProgress()
//     {
//         progressMap.Clear();

//         string raw = PlayerPrefs.GetString(ProgressKey, "");
//         if (string.IsNullOrEmpty(raw)) return;

//         string[] entries = raw.Split(',');
//         foreach (var entry in entries)
//         {
//             var parts = entry.Split(':');
//             if (parts.Length == 2 && int.TryParse(parts[1], out int val))
//             {
//                 progressMap[parts[0]] = val;
//             }
//         }
//     }
// }
