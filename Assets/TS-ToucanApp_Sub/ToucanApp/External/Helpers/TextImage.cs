using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ToucanApp.Data;

public class TextImage : MonoBehaviour, IResourceReceiver
{
    public Text textField;
    public Sprite[] sprites;
    public float spacing = 100;

    private void Awake()
    {
        textField = GetComponent<Text>();
    }

    public Vector3 GetPos(int charIndex)
    {
        string text = textField.text;

        if (charIndex < 0 || charIndex >= text.Length)
            return Vector3.zero;

        TextGenerator textGen = new TextGenerator(text.Length);
        Vector2 extents = textField.gameObject.GetComponent<RectTransform>().rect.size;
        textGen.Populate(text, textField.GetGenerationSettings(extents));

        int indexOfTextQuad = (charIndex * 4);

        Vector3 avgPos = (textGen.verts[indexOfTextQuad].position +
            textGen.verts[indexOfTextQuad + 1].position +
            textGen.verts[indexOfTextQuad + 2].position +
            textGen.verts[indexOfTextQuad + 3].position) / 4f;

        //avgPos.x = 0;
        return avgPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.TransformPoint(Vector3.zero), 10);
    }

    private IEnumerator ApplyImages()
    {
        int charIndex = 1;
        for (int i = 0; i < sprites.Length; i++)
        {
            float height = sprites[i].texture.height;
            float spaceSize = textField.lineSpacing * textField.fontSize;
            int newLines = Mathf.CeilToInt((float)height / spaceSize) / 2;

            string emptyLines = "";
            for (int j = 0; j < newLines; j++)
            {
                emptyLines += System.Environment.NewLine;
            }

            charIndex = textField.text.IndexOf('$', charIndex + 1);
            if (charIndex == -1)
                break;

            textField.text = textField.text.Remove(charIndex, 1);
            textField.text = textField.text.Insert(charIndex, emptyLines + "$" + emptyLines);
            charIndex = textField.text.IndexOf('$', charIndex + 1);
        }

        yield return 0;

        List<Vector3> positions = new List<Vector3>();
        charIndex = 1;
        while (true)
        {
            charIndex = textField.text.IndexOf('$', charIndex + 1);
            if (charIndex == -1)
                break;

            var pos = GetPos(charIndex);
            pos.x = 0;
            positions.Add(pos);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            var go = new GameObject("img");
            go.transform.SetParent(this.transform);
            go.transform.localPosition = positions[i];

            var image = go.AddComponent<Image>();
            image.sprite = sprites[i];
            var texture = image.sprite.texture;
            image.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
        }

        textField.text = textField.text.Replace("$", " ");
    }
        
    #region IContentReceiver implementation

    public void OnResourceChanged()
    {
        StartCoroutine(ApplyImages());
    }

    #endregion
}
