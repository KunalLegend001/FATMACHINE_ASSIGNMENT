using UnityEngine;

public class TrayManager : MonoBehaviour
{
    public static TrayManager Instance;  // Singleton reference

    private TrayGridBasedMover[] trays;   // All trays in scene

    public UIManager uiManager;       // Reference to your UI Manager

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        trays = FindObjectsOfType<TrayGridBasedMover>();
    }

    public void CheckAllTraysPlaced()
    {
        foreach (var tray in trays)
        {
            Debug.Log(tray);
            if (!tray.IsSnapped())
            {
                return; // At least one tray is not placed yet
            }
            
        }

        // All trays are placed!!
        Debug.Log("All trays placed! 🎉");

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }
        else
        {
            Debug.LogWarning("UI Manager is not assigned!");
        }
    }
}
