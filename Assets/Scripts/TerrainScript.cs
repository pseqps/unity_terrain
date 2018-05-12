﻿//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


public class TerrainScript : MonoBehaviour {

    public class Vector3Comparer : IComparer
    {
        public int Compare(object a, object b)
        {
            Vector3 c = (Vector3) a;
            Vector3 d = (Vector3) b;
            if (c.x > d.x) return 1;
            if (c.x < d.x) return -1; else return 0;
        }
    }

    Mesh mesh;
    Vector3[] terrMap;
	
    public int sliceSize;
    public float maxY;
    public float minY;
   // public GameObject explosion;

    // Use this for initialization
    void Start () {


        //mesh = new Mesh();
        /*
        terrMap = new Vector3[11]; //массив-карта
        terrMap[0] = new Vector3(-15, 0, 0);
        terrMap[1] = new Vector3(-8, 0, 0);
        terrMap[2] = new Vector3(-6, 0, 0);
        terrMap[3] = new Vector3(-4, 0, 0);
        terrMap[4] = new Vector3(-2, 0, 0);
        terrMap[5] = new Vector3(0, -3, 0);
        terrMap[6] = new Vector3(2, 0, 0);
        terrMap[7] = new Vector3(4, -3, 0);
        terrMap[8] = new Vector3(6, 0, 0);
        terrMap[9] = new Vector3(8, -3, 0);
        terrMap[10] = new Vector3(15, -3, 0);
        */
        /*
        terrMap = new Vector3[5]; //массив-карта
        terrMap[0] = new Vector3(-15, 0, 0);
        terrMap[1] = new Vector3(-8, 0, 0);
        terrMap[2] = new Vector3(0, 0, 0);
        terrMap[3] = new Vector3(8, 0, 0);
        terrMap[4] = new Vector3(15, 0, 0);
        */
		
        terrMap = new Vector3[5]; //массив-карта
        terrMap[0] = new Vector3(-8, 0, 0);
        terrMap[1] = new Vector3(-4, 0, 0);
        terrMap[2] = new Vector3(0, 0, 0);
        terrMap[3] = new Vector3(4, 0, 0);
        terrMap[4] = new Vector3(8, 0, 0);
        
        maxY = terrMap[0].y;
        for (int i = 0; i<terrMap.Length; i++)
        if (terrMap[i].y > maxY) maxY = terrMap[i].y;
		
        RecalculateTerr();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("ENTER");
    }
	
	void RecalculateTerr ()
	{
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();


        int vertLenght = terrMap.Length * 2 + 1; //количество вершин
        Vector3[] vertices = new Vector3[vertLenght];
        int[] tri = new int[vertLenght * 6 - 3]; //количество треугольников

        //цикл добавления вершин
        int j = 1;
        for (int i = 0; i < terrMap.Length; i++)
        {
            vertices[j] = terrMap[i];
            vertices[j + 1] = terrMap[i];
            vertices[j + 1].y = -5;
            j += 2;
        }

        //цикл добавления треугольников
        for (int i = 0; i < terrMap.Length * 2 - 1; i++)
        {
            j = i * 3;
            tri[j] = i + i % 2;
            tri[j + 1] = i + 1 - i % 2;
            tri[j + 2] = i + 2;
        }

        Vector2[] uv = new Vector2[vertLenght];
        uv[0] = new Vector2(0, 0);
		float x;// = vertices[1].x;
		float y;// = minY;;
		float dx = vertices[vertices.Length-1].x - vertices[1].x;
		//Debug.Log("0:" + vertices[1].x+" max:" + vertices[vertices.Length-1].x + " dx:"+ dx);
		float dy = maxY - minY;
		//Debug.Log(d);
		
		for (int i = 1; i < terrMap.Length*2; i+=2) {
		x = (vertices[i].x - vertices[1].x)/dx;
		y = (vertices[i].y - minY)/dy;
		
		uv[i] = new Vector2(x,y);
		uv[i+1] = new Vector2(x,0);
		}

        mesh.vertices = vertices;
        mesh.triangles = tri;
        //mesh.RecalculateNormals();
        mesh.uv = uv;

        TerrColliderHole();
    }

    // делаем дырку в земле
    public void TerrainHole (GameObject explosion, float explDiam)//Vector3[] terrMap)
    {

        Transform expTransf = explosion.GetComponent<Transform>();
        //float explDiam = expTransf.localScale.x;

		bool intersection = false;
		int rightCorner = 0;
		int leftCorner = 0;
		
        int i = 0;
        int firstDel = 0;
        float x1 = terrMap[0].x, x2 = terrMap[1].x;
        //float sliceX = expTransf.position.x - explDiam / 2;
        float sliceX = expTransf.position.x - explDiam / 2 - explDiam / sliceSize;
		float maxX = terrMap[terrMap.Length-1].x;
        ArrayList holeMap = new ArrayList();

        for (int j = 0; (j <= sliceSize); j++) { //выполняем, пока не переберем все слайсы взрыва
            
			sliceX += explDiam / sliceSize;

			//if (sliceX > terrMap[terrMap.Length-1].x) sliceX = terrMap[terrMap.Length-1].x; //обработка правого края
			// не работает!!! if (sliceX < terrMap[0].x) sliceX = terrMap[0].x; //обработка левого края
			if (sliceX > maxX) // обработка правого края
			{
				sliceX = maxX;
				j = sliceSize;
				rightCorner--;
			}
			
			if (sliceX < terrMap[0].x) {
				leftCorner = 1;
				sliceX = terrMap[0].x;
				continue; //обработка левого края
				
			}
			//if ((sliceX + explDiam / sliceSize) < terrMap[0].x) sliceX = terrMap[0].x;
			
            while (!((x1 <= sliceX) & (x2 >= sliceX)))
				{
                    i++;
                    x1 = terrMap[i].x;
                    //if (i == terrMap.Length-1) x2 = terrMap[i].x;
                    //else
                    x2 = terrMap[i + 1].x;
                }
            float ty1 = terrMap[i].y, ty2 = terrMap[i + 1].y;
            float terrY = ty2 - (ty2 - ty1) * (x2 - sliceX) / (x2 - x1); //уравнение отрезка
            float sqrt = (float)Math.Sqrt(Math.Abs(Math.Pow(explDiam / 2, 2) - Math.Pow(sliceX - expTransf.position.x, 2)));
            float sliceY;

                if (terrY > expTransf.position.y + sqrt) sliceY = terrY - 2 * sqrt;
                else sliceY = expTransf.position.y - sqrt; //уравнение окружности

			if (j == 0) firstDel = i; //первый элемент для удаления. Последний - i
			
            if (terrY > sliceY) {	
				holeMap.Add(new Vector3(sliceX, sliceY));
				intersection = true; //не делаем дырку, если взрыв над поверхностью
			}
			else holeMap.Add(new Vector3(sliceX, terrY));
        }
            //Debug.Log("========holeMap========");
			//foreach (Vector3 v in holeMap) Debug.Log(v.x+" "+v.y);

        if (intersection)
        {
/// ТУТ КОСЯК!!!
			Vector3[] rejectedMap = new Vector3[terrMap.Length - i + firstDel + rightCorner - leftCorner];
            Array.Copy(terrMap, 0, rejectedMap, 0, firstDel + 1 - leftCorner);	
            Array.Copy(terrMap, i + 1, rejectedMap, firstDel + 1 - leftCorner, terrMap.Length - i - 1 + rightCorner);//
		
			Debug.Log("========rejectedMap========");
			foreach (Vector3 v in rejectedMap) Debug.Log(v.x+" "+v.y);
            //Добавляем в мап
            Vector3[] holeMapArr = (Vector3[])holeMap.ToArray(typeof(Vector3));
            terrMap = new Vector3[rejectedMap.Length + holeMapArr.Length];
            Array.Copy(rejectedMap, terrMap, rejectedMap.Length);
            Array.Copy(holeMapArr, 0, terrMap, rejectedMap.Length, holeMapArr.Length);

            //сортируем результат
            IComparer vComparer = new Vector3Comparer();
            Array.Sort(terrMap, vComparer);

            /*
            //удаляем слишком узкие участки из карты
            holeMap.Clear();
            for (int j = 0; j < terrMap.Length-1; j++)
            {
                if (Math.Abs(terrMap[j + 1].x - terrMap[j].x) >= explDiam / sliceSize)
                {
                    holeMap.Add(terrMap[j]);
                   // Debug.Log("=========================================");
                }
                //Debug.Log(terrMap[j].x + " - " + terrMap[j + 1].x + " = " + (Math.Abs(terrMap[j + 1].x - terrMap[j].x)) + " > " + explDiam / sliceSize);
            }
            terrMap = (Vector3[])holeMap.ToArray(typeof(Vector3));
            Debug.Log(holeMap.Count+" "+terrMap.Length);
            */
			
            //делаем дырку в коллайдере
            RecalculateTerr();
			
            //foreach (Vector3 v in terrMap) Debug.Log(v.x+" "+v.y);
        }
    }

    void TerrColliderHole ()
    {
        EdgeCollider2D collider = GetComponent<EdgeCollider2D>();
        Vector2[] terrMap2d = new Vector2[terrMap.Length];
        for (int i = 0; i < terrMap.Length; i++) terrMap2d[i] = terrMap[i];
        collider.points = terrMap2d;
    }

    // Update is called once per frame
    void Update () {

        /* подвигать вершину
       Vector3[] vert = mesh.vertices;
       vert[3].x += .003f;
       mesh.vertices = vert;
       */
        //TerrainHole();
    }

}