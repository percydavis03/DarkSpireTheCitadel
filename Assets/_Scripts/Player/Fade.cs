using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    public CanvasGroup fadingCanvasGroup;

    bool isFaded;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Fades()
    {
        if (isFaded)
        {
            isFaded = false;
            print("fadeout");
        }
        else if (!isFaded)
        {
            isFaded = true;
            print("fadein");
        }

        if (isFaded)
        {
            fadingCanvasGroup.DOFade(1, 2);
            print("dofadein");
        }
        else
        {
            fadingCanvasGroup.DOFade(0, 2);
            print("dofadeout");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
