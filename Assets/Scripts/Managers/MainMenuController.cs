using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    void Start()
    {
        newGameButton.onClick.AddListener(NewGame);
        loadGameButton.onClick.AddListener(LoadSavedGame);
    }

    private void NewGame()
    {
        SceneManager.LoadScene("Main");
    }

    private void LoadSavedGame()
    {
        SaveManager.Instance.LoadData();
    }
}
