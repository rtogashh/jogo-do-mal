using UnityEngine;

public class impacto : MonoBehaviour
{
    [Tooltip("Tags que devem disparar o log 'acertou'")]
    public string[] targetTags = new string[] { "Enemy" };

    // Trigger (recomendado para hitboxes)
    private void OnTriggerEnter(Collider other)
    {
        if (IsTargetTag(other.gameObject))
            Debug.Log("acertou");
    }

    // Colisão física (caso não use trigger)
    private void OnCollisionEnter(Collision collision)
    {
        if (IsTargetTag(collision.gameObject))
            Debug.Log("acertou");
    }

    private bool IsTargetTag(GameObject go)
    {
        if (targetTags == null || targetTags.Length == 0)
            return false;

        foreach (var tag in targetTags)
        {
            if (!string.IsNullOrEmpty(tag) && go.CompareTag(tag))
                return true;
        }
        return false;
    }
}
