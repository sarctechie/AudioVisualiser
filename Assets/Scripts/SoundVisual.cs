using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(AudioSource))]

public class SoundVisual : MonoBehaviour
{
    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;
    private const int SAMPLE_SIZE=1024;

    public float backgroundIntensity;
    public Material backgroundMaterial;
    public Color minColor;
    public Color maxColor;

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    public float visualModifier=50.0f;
    public float smoothSpeed= 10.0f;

    private Transform[] visualList;
    private float[] visualScale;
    private int amnVisual=64;

    public float maxVisualScale=25.0f;
    public float keepPercentage= 0.5f;

   // Start is called before the first frame update
    private void Start()
    {
        source=GetComponent<AudioSource>() ;
        samples=new float[SAMPLE_SIZE];
        spectrum= new float[SAMPLE_SIZE];
        sampleRate=AudioSettings.outputSampleRate;

        //SpawnLine();
        SpawnCircle();
    }
    private void SpawnLine()
    {
        visualScale= new float[amnVisual];
        visualList= new Transform[amnVisual];

        for(int i=0; i<amnVisual; i++)
        {
            GameObject go= GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            visualList[i]=go.transform;
            visualList[i].position=Vector3.right*i;
        }
    }

    private void SpawnCircle()
    {
        visualScale= new float[amnVisual];
         visualList= new Transform[amnVisual];

        Vector3 center=Vector3.zero;
        float radius= 10.0f;

         for(int i=0; i<amnVisual; i++)
        {
            float ang=i*1.0f/amnVisual;
            ang= ang*Mathf.PI*2;

            float x= center.x+Mathf.Cos(ang)*radius;
            float y= center.y+Mathf.Sin(ang)*radius;

            Vector3 pos= center+new Vector3(x,y,0);
            GameObject go= GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            go.transform.position=pos;
            go.transform.rotation=Quaternion.LookRotation(Vector3.forward,pos);
            visualList[i]=go.transform;
        }

    }

    // Update is called once per frame
    private void Update()
    {
        AnalyzeSound();
        UpdateVisual();

        UpdateBackground();
    }

    private void UpdateVisual()
    {
        int visualIndex=0;
        int spectrumIndex=0;
        int averageSize=(int)((SAMPLE_SIZE*keepPercentage)/amnVisual);

        while (visualIndex<amnVisual)
        {
            int j=0;
            float sum=0;
            while(j<averageSize)
            {
                sum+=spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }

            float scaleY= sum/averageSize*visualModifier;
            visualScale[visualIndex]=Time.deltaTime*smoothSpeed;
            if(visualScale[visualIndex]<scaleY)
            visualScale[visualIndex]=scaleY;

            if(visualScale[visualIndex]>maxVisualScale)
            visualScale[visualIndex]=maxVisualScale;

            visualList[visualIndex].localScale=Vector3.one+Vector3.up*visualScale[visualIndex];
            visualIndex++;
        }
    }

    private void AnalyzeSound()
    {
        source.GetOutputData(samples,0);

       //RMS Calculation
        int i= 0;
        float sum=0;
        for(;i<SAMPLE_SIZE;i++)
        {
            sum+= samples[i]*samples[i];
        }
        rmsValue=Mathf.Sqrt(sum/SAMPLE_SIZE);

        //db Calculation
        dbValue= 20*Mathf.Log10(rmsValue/0.1f);

        //Spectrum
       source.GetSpectrumData(spectrum,0,FFTWindow.BlackmanHarris);

    }

    private void UpdateBackground()
    {
        backgroundIntensity-=Time.deltaTime*smoothSpeed;
        if(backgroundIntensity<dbValue/40)
           backgroundIntensity=dbValue/40;

           backgroundMaterial.color=Color.Lerp(maxColor,minColor,-backgroundIntensity);
    }
}
