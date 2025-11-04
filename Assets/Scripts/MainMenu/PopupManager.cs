using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private static PopupManager _instance;
    public static PopupManager Instance => _instance;

    private MonoBehaviour _currentlyOpenPopup = null;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestOpen(MonoBehaviour popup)
    {
        if (_currentlyOpenPopup != null && _currentlyOpenPopup != popup)
        {
            if (_currentlyOpenPopup is ICloseable closeable)
            {
                closeable.Close();
            }
            else
            {
                Debug.LogWarning("Popup does not implement ICloseable");
            }
        }

        _currentlyOpenPopup = popup;
    }

    public void NotifyClosed(MonoBehaviour popup)
    {
        if (_currentlyOpenPopup == popup)
        {
            _currentlyOpenPopup = null;
        }
    }
}

public interface ICloseable
{
    void Close();
}