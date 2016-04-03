﻿using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    private static int uniqueID; //Eventueel voor unieke identificatie van de tiles;
    private Renderer render;
    private GameManager gm;
    private GridGenerator gen;

    private Color defaultMaterialColor;
    public Color DefaultMaterialColor { get { return defaultMaterialColor; } }

    public Color attackColor = Color.magenta;
    public Color blockedColor = Color.red;
    public Color highlightColor = Color.yellow;
    public Color hoverColor = Color.green;
    public Color moveColor = Color.blue;
    public Color selectedColor = Color.green;

    public GameObject currentGameObject;
    public IBuildUnit buildUnit;
    public bool selected;
    public bool hover;
    public bool highlighted;
    public int ID;

    public int xTile;
    public int yTile;

    public Transform shape;

    void Awake()
    {
        render = gameObject.GetComponent<Renderer>(); //Verkrijgt de renderer van dit object.
        defaultMaterialColor = render.material.color; //Verkrijgt de huidige kleur van dit object.
        selected = false; //Kijkt of de tile ingedrukt is of niet.
        uniqueID = uniqueID + 1;
        ID = uniqueID;
    }

    /// <summary>
    /// Wordt uitgevoerd wanneer er een muis over een tile hovert.
    /// </summary>
    public void MouseHover()
    {
        if (selected == false)
        {
            HoverTile();
        }
    }

    /// <summary>
    /// Wordt uitgevoerd wanneer er op dit tile geklikt word.
    /// </summary>
    public void MouseClick()
    {
        SelectTile();
    }

    /// <summary>
    /// Wordt uitgevoerd wanneer een andere tile geselecteerd word.
    /// </summary>
    public void MouseExit()
    {
        if (highlighted)
        {
            ColorTile(TileColor.Highlight);
        }
        if (!selected)
        {
            ColorTile(TileColor.Default);
            if(buildUnit != null)
            {
                HighLightNearbyTiles(buildUnit.GetRange(), buildUnit.GetRangeType(), false);
            }
        }
    }

    /// <summary>
    /// Wordt uitgevoerd wanneer de mouse op een tile zit.
    /// </summary>
    public void HoverTile()
    {
        if (!highlighted && buildUnit != null)
        {
            if (buildUnit.Player.ID == gm.GetPlayerController.currentPlayer.ID)
            {
                ColorTile(TileColor.Hover);
            }
            else
            {
                ColorTile(TileColor.Blocked);
            }
        }
        else if (highlighted)
        {
            if (buildUnit != null)
            {
                if (buildUnit.Player.ID == gm.GetPlayerController.currentPlayer.ID)
                {
                    ColorTile(TileColor.Hover);
                    HighLightNearbyTiles(buildUnit.GetRange(), buildUnit.GetRangeType(), true);
                }
                else
                {
                    ColorTile(TileColor.Attack);
                }
            }
            else
            {
                ColorTile(TileColor.Move);
            }
        }
        else
        {
            ColorTile(TileColor.Hover);
        }
    }

    /// <summary>
    /// Geeft de selecteer kleur aan de gegeven tile.
    /// </summary>
    public void SelectTile()
    {
        ResetGameManagerTile();
        selected = !selected;
        if (buildUnit != null)
        {
            if (buildUnit.Player.ID == gm.GetPlayerController.currentPlayer.ID)
            {
                ColorTile(TileColor.Selected);
                HighLightNearbyTiles(buildUnit.GetRange(), buildUnit.GetRangeType(), true);
            }
            else
            {
                ColorTile(TileColor.Blocked);
            }
        }
        else
        {
            ColorTile(TileColor.Selected);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <param name="type"></param>
    /// <param name="highlight"></param>
    public void HighLightNearbyTiles(int range, RangeType type, bool highlight)
    {
        if (range > 0)
        {
            gen = GridGenerator.GetGridGenerator();

            int minX = xTile - range;
            int minY = yTile - range;
            int maxX = xTile + range;
            int maxY = yTile + range;
            if (minX < 0) minX = 0;
            if (minY < 0) minY = 0;
            if (maxX >= gen.terrainWidth) maxX = gen.terrainWidth - 1;
            if (maxY >= gen.terrainHeight) maxY = gen.terrainHeight - 1;

            if (type == RangeType.Cross)
            {
                for (int x = minX; x <= maxX; x++) //Todo: niet door drawen wanneer er iets in de weg zit en vanuit het object loopen, zodat je makkelijk uit de for loop kan gaan wanneer een object in de weg zit.
                {
                    gen.terrain[x, yTile].HighlightTile(highlight);
                }
                for (int y = minY; y <= maxY; y++)
                {
                    gen.terrain[xTile, y].HighlightTile(highlight);
                }
            }
            else
            {
                //Formule voor sphere highlighting
            }
        }
        else
        {
            Debug.Log("Range is 0 of" + this + " " + ID);
        }
    }

    /// <summary>
    /// Highlights the current tile.
    /// </summary>
    /// <param name="highlight">If this object should be highlighted or not</param>
    public void HighlightTile(bool highlight)
    {
        highlighted = highlight;
        if (highlighted)
        {
            ColorTile(TileColor.Highlight);
        }
        else
        {
            ColorTile(TileColor.Default);
        }
    }

    /// <summary>
    /// Geeft de tile een van de 7 mogelijke kleuren.
    /// </summary>
    /// <param name="color">De kleur die de tile moet worden!</param>
    public void ColorTile(TileColor color)
    {
        switch (color)
        {
            case TileColor.Hover:
                render.material.color = hoverColor;
                break;
            case TileColor.Selected:
                render.material.color = selectedColor;
                break;
            case TileColor.Blocked:
                render.material.color = blockedColor;
                break;
            case TileColor.Highlight:
                render.material.color = highlightColor;
                break;
            case TileColor.Move:
                render.material.color = moveColor;
                break;
            case TileColor.Attack:
                render.material.color = attackColor;
                break;
            default:
            case TileColor.Default:
                render.material.color = defaultMaterialColor;
                break;
        }
    }

    /// <summary>
    /// Reset de huidig geselecteerde tile in de gamemanager.
    /// </summary>
    public void ResetGameManagerTile()
    {
        gm = GameManager.GetGameManager();
        if (gm.selectedTile != null)
        {
            Tile t = gm.selectedTile;
            if (t != null && t.ID != ID)
            {
                t.ResetTile();
                gm.selectedTile = this;
            }
        }
        else
        {
            gm.selectedTile = this;
        }
    }

    /// <summary>
    /// Reset de tile naar zijn default color.
    /// </summary>
    public void ResetTile()
    {
        selected = false;
        ColorTile(TileColor.Default);
        if (buildUnit != null)
        {
            HighLightNearbyTiles(buildUnit.GetRange(), buildUnit.GetRangeType(), false);
        }
    }

    /// <summary>
    /// Zet de coördinaten van tile vast in paramaters.
    /// </summary>
    /// <param name="x">x coörd van de tile</param>
    /// <param name="y">y coörd van de tile</param>
    public void SetCoords(int x, int y)
    {
        xTile = x;
        yTile = y;
    }

    /// <summary>
    /// Spawnt een object op deze tile
    /// </summary>
    /// <param name="g">Het game object wat je wilt spawnen</param>
    /// <returns>Het gespawnde GameObject (Een clone van het meegegeven GameObject)</returns>
    public GameObject SpawnObject(GameObject g)
    {
        if (currentGameObject != null)
        {
            Debug.Log("There already is an object on this tile!");
            return currentGameObject;
        }
        else
        {
            currentGameObject = Instantiate(g);
            currentGameObject.transform.parent = transform;
            currentGameObject.transform.position = transform.parent.position;
            currentGameObject.transform.localPosition = new Vector3(0f, 0f, (currentGameObject.GetComponent<Renderer>().bounds.size.y / 2));
            currentGameObject.transform.rotation = Quaternion.identity;
            if (currentGameObject != null)
            {
                buildUnit = currentGameObject.GetComponent<IBuildUnit>();
            }

            Debug.Log(currentGameObject);
            return currentGameObject;
        }
    }
}
