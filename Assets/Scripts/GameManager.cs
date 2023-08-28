using Checkers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlaybleSide playbleSide;
    [SerializeField] private CameraMover cameraMover;

    [SerializeField] private Material cellHighlightMaterial;
    [SerializeField] private Material nextCellMaterial;
    [SerializeField] private Material chipHighlightMaterial;
    [SerializeField, Range(0, 5)] private float chipSpeed;

    private CellComponent[,] cells = new CellComponent[8, 8];
    [SerializeField] public List<ChipComponent> chips;
    
    private ChipComponent selectedChip;
    private CellComponent selectedCell;

    private bool isMoving = false;

    public event Action ObjectMoved;

    public event Action<ColorType> GameEnded;

    private void Awake()
    {
        LinkCellsToArray();
        ChipsPair();
        LinksCellNeighor();
    }

    private void OnEnable()
    {
        CellsHighlight(true);
    }

    private void OnDisable()
    { 
        CellsHighlight(false);
    }
    private void LinkCellsToArray()
    {
        float cellSize = 1;

        Vector3 startPos = new Vector3(0, 0, 0);

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Vector3 cellPos = startPos + new Vector3(col * cellSize, 0, row * cellSize);

                Collider[] colliders = Physics.OverlapSphere(cellPos, cellSize / 3, LayerMask.GetMask("Default"));

                if (colliders.Length > 0)
                {
                    CellComponent cellComponent = colliders[0].GetComponent<CellComponent>();
                    cellComponent.name = $"Cell X:{row + 1} Y:{col + 1}";

                    cells[row, col] = cellComponent;
                }
            }
        }
    }

    private void ChipsPair()
    {
        Vector3 startPos = new Vector3(0, 0.15f, 0);

        float chipSize = 1;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                CellComponent cell = cells[row, col];

                if (cell.GetColor == ColorType.Black)
                {
                    Vector3 cellPos = startPos + new Vector3(col * chipSize, 0, row * chipSize);

                    Collider[] colliders = Physics.OverlapSphere(cellPos, chipSize / 3, LayerMask.GetMask("Chips"));

                    if (colliders.Length > 0)
                    {
                        ChipComponent chip = colliders[0].GetComponent<ChipComponent>();

                        if (chip.GetColor == ColorType.Black)
                        {
                            chip.name = $"ChipBlack X:{row} Y:{col}";
                            cells[row,col].Pair = chip;                         
                        }
                        else
                        {
                            chip.name = $"ChipWhite X:{row} Y:{col}";
                            cells[row, col].Pair = chip;                           
                        }
                    
                    }
                }
            }
        }
    }

    private void ClearAndRePairDesk()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                cells[row, col].Pair = null;
                cells[row,col].RemoveAdditionalMaterial(1);
                cells[row, col].RemoveAdditionalMaterial(2);
            }
        }

        foreach (var chip in chips)
        {
            chip.RemoveAdditionalMaterial(1);
        }

        ChipsPair();
    }

    private void LinksCellNeighor()
    {
        for(int row = 0;row < 8; row++) 
        {
            for(int  col = 0; col < 8;col++) 
            {
                CellComponent cell = cells[row,col];
                
                if(cell.GetColor == ColorType.Black)
                {
                    Dictionary<NeighborType,CellComponent> neighbor = new Dictionary<NeighborType, CellComponent> ();
                    neighbor[NeighborType.TopLeft] = GetNeighbor(row - 1, col - 1);
                    neighbor[NeighborType.TopRight] = GetNeighbor(row - 1, col + 1);
                    neighbor[NeighborType.BottomLeft] = GetNeighbor(row + 1, col - 1);
                    neighbor[NeighborType.BottomRight] = GetNeighbor(row + 1, col + 1);

                    cell.Configuration(neighbor);
                }
            }
        }
    }

    private CellComponent GetNeighbor(int row , int col)
    {
        if (row >= 0 && row < 8 && col >= 0 && col < 8)
        {
           return cells[row,col];
        }
        else
        { 
            return null; 
        }
    }

    private void CellsHighlight(bool switcher)
    {
        if(switcher == true)
        { 
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    cells[row, col].OnClickEventHandler += CellClick;
                    cells[row, col].OnFocusEventHandler += OnFocusCell;
                    cells[row, col].OnClickEventHandler += NextCell;
                }
            }
        }
        else if (switcher == false)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    cells[row, col].OnClickEventHandler -= CellClick;
                    cells[row, col].OnFocusEventHandler -= OnFocusCell;
                    cells[row, col].OnClickEventHandler -= NextCell;
                }
            }
        }
    } 

    private void OnFocusCell(CellComponent component, bool isSelect)
        {
            if (component.GetColor != ColorType.White)
            {
                component.AddAdditionalMaterial(cellHighlightMaterial, 1);
            }

            if (isSelect == false)
            {
                component.RemoveAdditionalMaterial(1);
            }
        }

    private void CellClick(BaseClickComponent clickedComponent)
    {
        CellComponent cell = clickedComponent as CellComponent;
        if (cell != null && cell.Pair != null)
        {
            if (playbleSide.CurrectSide != cell.Pair.GetColor)
            {
                Debug.LogError("Ходит другой игрок!");
                return;
            }

           if (selectedChip != null)
           {
              selectedChip.RemoveAdditionalMaterial();
           }

           ChipComponent chip = cell.Pair as ChipComponent;
           if (chip != null)
           {
               CellsHighlight(cell);
               cell.Pair.AddAdditionalMaterial(chipHighlightMaterial, 1);
               selectedChip = chip;          
           }
        }
    }

    private void CellsHighlight(CellComponent cell)
    {
        CellComponent leftCell;
        CellComponent rightCell;

        var selectedCell = cell;

        if (selectedCell != null)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    cells[row, col].RemoveAdditionalMaterial(2);
                }
            }
        }

        if (playbleSide.CurrectSide == ColorType.White)
        {
            leftCell = cell.GetNeighbors(NeighborType.BottomLeft);
            rightCell = cell.GetNeighbors(NeighborType.BottomRight);
        
            if(leftCell != null) 
            {
                if(leftCell.Pair == null)
                {
                    leftCell.AddAdditionalMaterial(nextCellMaterial, 2);
                }
                else
                {
                    if (leftCell.GetNeighbors(NeighborType.BottomLeft).Pair == null)
                    {
                        leftCell.GetNeighbors(NeighborType.BottomLeft).AddAdditionalMaterial(nextCellMaterial, 2);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }

            if (rightCell != null)
            {
                if (rightCell.Pair == null)
                {
                    rightCell.AddAdditionalMaterial(nextCellMaterial, 2);
                }
                else
                {
                    if (rightCell.GetNeighbors(NeighborType.BottomRight).Pair == null)
                    {
                        rightCell.GetNeighbors(NeighborType.BottomRight).AddAdditionalMaterial(nextCellMaterial, 2);
                    }
                }
            }

        }
        else
        {
            leftCell = cell.GetNeighbors(NeighborType.TopLeft);
            rightCell = cell.GetNeighbors(NeighborType.TopRight);

            if (leftCell != null)
            {
                if (leftCell.Pair == null)
                {
                    leftCell.AddAdditionalMaterial(nextCellMaterial, 2);
                }
                else
                {
                    if (leftCell.GetNeighbors(NeighborType.TopLeft).Pair == null)
                    {
                        leftCell.GetNeighbors(NeighborType.TopLeft).AddAdditionalMaterial(nextCellMaterial, 2);
                    }
                }
            }

            if (rightCell != null)
            {
                if (rightCell.Pair == null)
                {
                    rightCell.AddAdditionalMaterial(nextCellMaterial, 2);
                }
                else
                {
                    if (rightCell.GetNeighbors(NeighborType.TopRight).Pair == null)
                    {
                        rightCell.GetNeighbors(NeighborType.TopRight).AddAdditionalMaterial(nextCellMaterial, 2);
                    }
                }
            }

        }
    }
 
    private void NextCell(BaseClickComponent cell)
    {
        if (!isMoving)
        {
            CellComponent nextCell = cell as CellComponent;
            if (cell.Pair == null && cell.GetColor != ColorType.White && HasMaterialAtIndex(nextCell))
            {
                selectedCell = nextCell;
                StartCoroutine(Move(selectedCell));
            }
        }
    }

    private bool HasMaterialAtIndex(CellComponent cell)
    {
        MeshRenderer mesh = cell.GetComponent<MeshRenderer>();
        if(mesh != null && mesh.sharedMaterials.Length > 2)
        {
            return mesh.sharedMaterials[2] == nextCellMaterial;
        }
        return false;
    }

    private IEnumerator Move(BaseClickComponent nextCell)
    {
        isMoving = true;

        Vector3 startPosition = selectedChip.transform.position;
        Vector3 targetPosition = nextCell.transform.position;
        Vector3 endPostion = new Vector3 (targetPosition.x , startPosition.y , targetPosition.z);


        float journeyLenght = Vector3.Distance(startPosition, endPostion);
        float startTime = Time.time;

        var eventSystem = EventSystem.current;
        eventSystem.gameObject.SetActive(false);
        
        while (selectedChip.transform.position != endPostion)
        {
            float distanceCovered = (Time.time - startTime) * chipSpeed;
            float fractionOfJourney = distanceCovered / journeyLenght;

            selectedChip.transform.position = Vector3.Lerp(startPosition, endPostion, fractionOfJourney);
            yield return null;
        }

        selectedChip.transform.position = endPostion;
        ClearAndRePairDesk();
        isMoving = false;

        switch(playbleSide.CurrectSide)
        {
            case ColorType.White when nextCell.transform.position.z == 7:
                GameEnded?.Invoke(ColorType.White);
                yield break;

            case ColorType.Black when nextCell.transform.position.z == 0:
                GameEnded?.Invoke(ColorType.Black);
                yield break;
        }

        yield return StartCoroutine(cameraMover.OnCameraMove());

        ObjectMoved?.Invoke();
        eventSystem.gameObject.SetActive(true);
    }

}


    