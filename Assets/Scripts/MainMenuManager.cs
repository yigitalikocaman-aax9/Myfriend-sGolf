using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Yükleniyor Ekranı (İsteğe Bağlı)")]
    public GameObject loadingPanel; // Eğer varsa ekrandaki "Yükleniyor..." panelini buraya bağlayabilirsin

    public void StartGame()
    {
        // Sahneyi arkada yüklemesi için Coroutine (eşzamanlı iş parçacığı) başlatıyoruz
        StartCoroutine(LoadSceneAsyncCoroutine("MyCity"));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        // Eğer bir yükleniyor ekranı tasarladıysan onu aktif et
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Sahneyi arka planda yüklemeye başla
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Sahne tamamen yüklenene kadar buradaki döngü çalışır (oyun donmaz)
        while (!operation.isDone)
        {
            // İleride buraya bir yüklenme barı (Progress Bar) yapmak istersen:
            // float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            yield return null; // Bir sonraki kareye kadar bekle
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyundan çıkıldı!");
    }
}