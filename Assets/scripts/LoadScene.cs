using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField]
    GameObject LoadScreen_screen;
    [SerializeField]
    Image LoadFill;

    public void LoadarScene(int cena)
    {
        StartCoroutine(LoadSceneAsync(cena));
    }

    IEnumerator LoadSceneAsync(int cena)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(cena);
        LoadScreen_screen.SetActive(true);
        while (!operation.isDone)
        {
            float progresso = Mathf.Clamp01(operation.progress/0.9f);
            LoadFill.fillAmount = progresso;
            yield return null;
        }
        LoadScreen_screen.SetActive(false);
    }
}
