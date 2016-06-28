﻿using UnityEngine;
using System.Collections;
using System;

public class Unit : MonoBehaviour, IBuildUnit
{

    public UnitType unitType;
    public RangeType rangeType;
    public int range;
    public int health;
    public int damage;
    public int armor;
    public bool moveAble;
    public bool canAttack;
    public int[] unitCost;
    public int[] resourcesIncrease;
    private int amountOfResources;
    public float movementSpeed;
    public float withinBoundry = 0.70f;
    public float distanceBoundry = 0.80f;

    private Renderer render;
    private GameManager gm;

    public Animator ani;
    public Player player;
    public GameObject unit;
    public Tile currentTile;
    public Vector3 currentPosition;

    public Color selectedColor = Color.cyan;
    public Color hoverColor = Color.magenta;
    public Color blockedColor = Color.grey;
    public Color attackColor = Color.red;

    private Color defaultMaterialColor;
    public Color DefaultMaterialColor { get { return defaultMaterialColor; } }

    private static int uniqueID;
    public int ID;

    public bool moving;
    public bool attacking;

    public Player Player
    {
        get
        {
            return player;
        }
    }

    void Awake()
    {
        render = gameObject.GetComponentInChildren<Renderer>(); //Verkrijgt de renderer van dit object.
        defaultMaterialColor = render.material.color;
        gm = GameManager.GetGameManager();
        uniqueID = uniqueID + 1;
        ID = uniqueID;
        CheckUnitCostArrayLength();
        CheckResourceIncreaseArrayLength();
        unit = this.gameObject;
        ani = GetComponentInParent<Animator>();
        ani.SetFloat("Input X", 0);
        ani.SetFloat("Input Z", 0);
        ani.SetBool("Moving", false);
        ani.SetBool("Running", false);
    }

    void Start()
    {
        currentPosition = transform.position;
    }

    void Update()
    {
        Debug.Log(gameObject.transform.position);
        if ((gameObject.transform.position.x >= (currentPosition.x + withinBoundry) ||
            gameObject.transform.position.x <= (currentPosition.x - withinBoundry) ||
            gameObject.transform.position.z >= (currentPosition.z + withinBoundry) ||
            gameObject.transform.position.z <= (currentPosition.z - withinBoundry)) &&
            currentPosition != null) 
        {
            transform.LookAt(currentPosition);
            float distance = Vector3.Distance(transform.position, currentPosition);
            Debug.Log(distance);
            if (distance <= distanceBoundry)
            {
                transform.position = currentPosition;
                moving = false;
                if (ani != null)
                {
                    ani.SetBool("Moving", false);
                    ani.SetBool("Running", false);
                }
            }
            else
            {
                //transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
            }
        }
    }


    public void CheckUnitCostArrayLength()
    {
        amountOfResources = GameManager.amountOfResources;
        if (unitCost.Length < amountOfResources)
        {
            Debug.LogError("Array length needs to be:" + amountOfResources);
            unitCost = new int[amountOfResources];
        }
        else if (unitCost.Length > amountOfResources)
        {
            Debug.LogError("The array is a bit too long so not all values will be used!");
            unitCost = new int[amountOfResources];
        }
    }

    public void CheckResourceIncreaseArrayLength()
    {
        amountOfResources = GameManager.amountOfResources;
        if (resourcesIncrease.Length < amountOfResources)
        {
            Debug.LogError("Array length needs to be:" + amountOfResources);
            resourcesIncrease = new int[amountOfResources];
        }
        else if (resourcesIncrease.Length > amountOfResources)
        {
            Debug.LogError("The array is a bit too long so not all values will be used!");
            resourcesIncrease = new int[amountOfResources];
        }
    }


    public void OnSpawn(Tile t, Player p)
    {
        currentTile = t;
        player = p;
        for (int i = 0; i < unitCost.Length; i++)
        {
            if (p.resources[i] >= unitCost[i])
            {
                p.resources[i] -= unitCost[i];
            }
            else
            {
                Debug.LogError("A object has been spawned that has a higher cost than the players resources!");
            }
        }
        Debug.Log("Tile ID:" + t.ID + " player:" + p.playerName);
    }


    public void Select()
    {
        if (currentTile.ID == gm.selectedTile.ID)
        {
            if (gm.GetPlayerController.currentPlayer.ID != currentTile.buildUnit.Player.ID)
            {
                ColorObject(BuildUnitColor.Blocked);
            }
            else
            {
                ColorObject(BuildUnitColor.Selected);
            }
        }
        else
        {
            currentTile.SelectTile();
        }
    }

    public void Hover()
    {
        if (!currentTile.hover)
        {
            currentTile.MouseHover();
        }
        if (currentTile.hover && !currentTile.selected && !currentTile.highlighted)
        {
            if (gm.GetPlayerController.currentPlayer.ID != currentTile.buildUnit.Player.ID)
            {
                ColorObject(BuildUnitColor.Blocked);
            }
            else
            {
                ColorObject(BuildUnitColor.Hover);
            }
        }
    }

    public void Exit()
    {
        if (gm.GetPlayerController.currentPlayer.ID == player.ID)
        {
            ColorObject(BuildUnitColor.Default);
        }
        else
        {
            ColorObject(BuildUnitColor.Blocked);
        }
    }

    public void Blocked()
    {
        ColorObject(BuildUnitColor.Blocked);
    }

    public int GetRange()
    {
        return range;
    }

    public RangeType GetRangeType()
    {
        return rangeType;
    }

    public void ColorObject(BuildUnitColor col)
    {
        switch (col)
        {
            case BuildUnitColor.Hover:
                render.material.color = hoverColor;
                break;
            case BuildUnitColor.Selected:
                render.material.color = selectedColor;
                break;
            case BuildUnitColor.Blocked:
                render.material.color = blockedColor;
                break;
            case BuildUnitColor.Attack:
                render.material.color = attackColor;
                break;
            default:
            case BuildUnitColor.Default:
                render.material.color = defaultMaterialColor;
                break;
        }
    }

    public bool TileHighlighted()
    {
        if (currentTile != null)
        {
            return currentTile.highlighted;
        }
        else
        {
            return false;
        }
    }

    public Tile getTile()
    {
        return currentTile;
    }

    public bool Move(Tile t, bool turn)
    {
        //snelle movement test;
        if (t.buildUnit == null && moveAble)
        {
            if (currentTile.selected)
            {
                currentTile.MouseExit();
                currentTile.ResetTile();
                t.buildUnit = this;
                currentTile.buildUnit = null;
                currentTile = t;
                currentTile.MouseClick();
                moving = true;
                if (ani != null)
                {
                    ani.SetBool("Moving", true);
                    ani.SetBool("Running", true);
                }
                currentPosition = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y, currentTile.transform.position.z); //y + (render.bounds.size.y / 2) voor andere objecten

                if (turn)
                    gm.GetPlayerController.Turn();

                //currentTile.gameObject = this;
                //float y = currentTile.transform.position.y;

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool CanMove()
    {
        return moveAble;
    }

    public bool Attack(Tile t)
    {
        if (t.buildUnit != null && canAttack)
        {
            if(t.buildUnit.GetRange() >= 2)
            ani.SetTrigger("Attack1Trigger");
            attacking = true;
            transform.LookAt(new Vector3(t.transform.position.x, t.transform.position.y, t.transform.position.z));
            if (!t.buildUnit.Defend(damage))
            {
                t.buildUnit = null;
                gm.GetPlayerController.Turn();
            }
            else
            {
                gm.GetPlayerController.Turn();
            }
            return true;
        }
        else
        {
            gm.GetPlayerController.Turn();
            return false;
        }
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public bool Defend(int damage)
    {
        if (damage >= health)
        {
            player.RemoveUnit(this);
            Destroy(gameObject);
            return false;
        }
        else
        {
            if (armor >= damage)
            {
                armor -= damage;
                return true;
            }
            else if (armor > 0)
            {
                int remainingDamage = damage - armor;
                armor = 0;
                health -= remainingDamage;
                if (health <= 0)
                {
                    player.RemoveUnit(this);
                    Destroy(gameObject);
                    return false;
                }
                return true;
            }
            else
            {
                health -= damage;
                return true;
            }
        }
    }

    public bool Upgrade()
    {
        throw new NotImplementedException();
    }

    public bool IsMoving()
    {
        throw new NotImplementedException();
    }
}
