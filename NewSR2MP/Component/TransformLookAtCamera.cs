namespace NewSR2MP.Component;

[RegisterTypeInIl2Cpp(false)]
public class TransformLookAtCamera : MonoBehaviour
{
    public Transform targetTransform;

    void Update()
    {
        if (Camera.main != null)
        {
            targetTransform.LookAt(Camera.main.transform);
        }
        
    }
}