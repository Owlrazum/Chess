using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    [SerializeField]
    private Material selectedMaterial;

    [SerializeField]
    private Material allowedMaterial;

    [SerializeField]
    private Material attackedMaterial;

    [SerializeField]
    private Material testWhiteMaterial;

    [SerializeField]
    private Material testBlackMaterial;

    [SerializeField]
    private Material kingThreatedMaterial;
    
    private Material defaultMaterial;


    private void Start()
    {
        defaultMaterial = GetComponent<Renderer>().material;
    }

    // Start is called before the first frame update
    public void Select()
    {
        GetComponent<Renderer>().material = selectedMaterial;
    }
    public void Allow()
    {
        GetComponent<Renderer>().material = allowedMaterial;        
    }
    public void Threat()
    {
        GetComponent<Renderer>().material = attackedMaterial;
    }
    public void ThreatKing()
    {
        GetComponent<Renderer>().material = kingThreatedMaterial;
    }
    public void Default()
    {
        GetComponent<Renderer>().material = defaultMaterial;
    }

    public void TestWhite()
    {
        GetComponent<Renderer>().material = testWhiteMaterial;
    }
    public void TestBlack()
    {
        GetComponent<Renderer>().material = testBlackMaterial;
    }
}
