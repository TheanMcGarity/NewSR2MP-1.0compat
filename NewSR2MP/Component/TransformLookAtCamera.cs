using Il2CppTMPro;

namespace NewSR2MP.Component;

[RegisterTypeInIl2Cpp(false)]
public class TransformLookAtCamera : MonoBehaviour
{
    public Transform targetTransform;

    private bool isText;

    void Start() => isText = targetTransform.GetComponent<TextMesh>();
    
    void Update()
    {
        if (Camera.main != null)
        {
            targetTransform.LookAt(Camera.main.transform);

            if (isText)
                targetTransform.Rotate(0, 180, 0);
        }
        
    }
}