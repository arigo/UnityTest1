using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum Kinds { White, Gray, Yellow, Cyan };


public class GlobalData : MonoBehaviour {

    public Color[] kind2Color;
    int m_interacting = 0;
    List<int> m_counters = new List<int>();

    private static GlobalData s_Instance = null;

    public static GlobalData instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = FindObjectOfType(typeof(GlobalData)) as GlobalData;

            if (s_Instance == null)
                Debug.LogError("No GlobalData instance found");

            return s_Instance;
        }
    }

    // Ensure that the instance is destroyed when the game is stopped in the editor.
    void OnApplicationQuit()
    {
        s_Instance = null;
    }

    public void InteractionStart()
    {
        if (m_interacting == 0)
            BroadcastMessage("MsgInteractionStart", SendMessageOptions.DontRequireReceiver);
        m_interacting++;
    }

    public void InteractionStop()
    {
        for (int i = 0; i < m_counters.Count; i++)
            m_counters[i] = 0;

        if (m_interacting > 0)
            m_interacting--;
    }

    public bool Interacting()
    {
        return m_interacting > 0;
    }

    public int CreateCounter()
    {
        int result = m_counters.Count;
        m_counters.Add(0);
        return result;
    }

    public int GetCounter(int index)
    {
        if (m_interacting == 0)
            return m_counters[index];
        else
            return -1;
    }

    public void SetCounter(int index, int value)
    {
        if (m_interacting == 0)
        {
            m_counters[index] = value;

            bool is_finished = true;
            for (int i = 0; i < m_counters.Count; i++)
                is_finished = is_finished && (m_counters[i] >= 10);

            if (is_finished)
                Invoke("LevelFinished", 1.0f);
        }
    }

    void LevelFinished()
    {
        int cur = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(cur + 1);
    }
}