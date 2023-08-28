using Checkers;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlaybleSide : MonoBehaviour
{
    public ColorType CurrectSide { get; private set; } = ColorType.White;

    [SerializeField] private GameManager gameManager;

    List<BaseClickComponent> chipBlack = new();
    List<BaseClickComponent> chipWhite = new();

    private void OnEnable()
    {
        gameManager.ObjectMoved += OnChangeSide;
        gameManager.GameEnded += GameEnded;
    }

    private void OnDisable()
    {
        gameManager.ObjectMoved -= OnChangeSide;
        gameManager.GameEnded -= GameEnded;
    }

    private void Start()
    {
        foreach (var chip in gameManager.chips)
        {
            if(chip.GetColor == ColorType.White)
            {
                chipWhite.Add(chip);
            }
            else
            {
                chipBlack.Add(chip);
            }    
        }
    }

    private void ChipDestroy(BaseClickComponent chip)
    {
        if(chipWhite.Contains(chip))
        {
            chipWhite.Remove(chip);
        }
        else
        {
            chipBlack.Remove(chip);
        }

        if(chipWhite.Count == 0)
        {
            GameEnded(ColorType.White);
        }
        else if (chipBlack.Count == 0)
        {
            GameEnded(ColorType.Black);
        }
    }

    private void OnChangeSide()
    {
        if(CurrectSide == ColorType.White)
        {
            CurrectSide = ColorType.Black;
            Debug.Log("Ход чёрных");

            foreach(var chip in gameManager.chips) 
            {
                if (chip.GetColor == ColorType.White)
                {
                    chip.gameObject.tag = "Enemy";
                }
                else
                {
                    chip.gameObject.tag = "Untagged";
                }
            }
        }
        else 
        {
            CurrectSide = ColorType.White;
            Debug.Log("Ход белых");

            foreach (var chip in gameManager.chips)
            {
                if (chip.GetColor == ColorType.Black)
                {
                    chip.gameObject.tag = "Enemy";
                }
                else
                {
                    chip.gameObject.tag = "Untagged";
                }
            }
        }
    }

    private void GameEnded(ColorType side)
    {
        string victorySide;
        if (side == ColorType.White)
        {
            victorySide = "белой";
        }
        else
        {
            victorySide = "чёрной";
        }

        Debug.Log($"Победа {victorySide} стороны!");
        EditorApplication.isPaused = true;
    }
}
