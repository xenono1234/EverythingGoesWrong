using UnityEngine;

[DisallowMultipleComponent]
public class TagIdentifier : MonoBehaviour
{
    // The computed tag ID for this object.
    public int tagID;

    void Awake()
    {
        tagID = GetTagIDFromTag(gameObject.tag);

        // Set this value in a MaterialPropertyBlock so that it’s available per object.
        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat("_ObjectTagID", tagID);
            rend.SetPropertyBlock(mpb);
        }
    }

    int GetTagIDFromTag(string tag)
    {
        // You can define your own mapping here. For now we simply use GetHashCode.
        return tag.GetHashCode();
    }
}
