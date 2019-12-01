using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{

    public Piece[,] pieces = new Piece[8, 8];

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private bool isWhite;
    private bool isWhiteTurn;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private Piece selectedPiece;

    // Start is called before the first frame update
    private void Start()
    {
        isWhiteTurn = true;
        GenerateBoard();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateMouseOver();

        //if my turn
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(1))
            {
                selectedPiece = null;
            }
            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
            }
            if(Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y); 
            }
        }
    }
    private void UpdateMouseOver()
    {
        if(!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private void UpdatePieceDrag(Piece p)
    {
        if(!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void SelectPiece(int x, int y)
    {
        //out of bounds
        if (x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length)
            return;
        Piece p = pieces[x, y];
        if (p != null)
        {
            selectedPiece = p;
            startDrag = mouseOver;
            Debug.Log(selectedPiece.name);
        }
    }
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        // Multimpayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        // Out of bounds
        if (x2 < 0 || x2 >= pieces.Length || y2 < 0 || y2 >= pieces.Length)
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1);

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if(selectedPiece != null)
        {
            // If it has not moved
            if(endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // Check if its a valid move
            if(selectedPiece.ValidMove(pieces,x1,y1,x2,y2))
            {
                // Did we kill anything
                // If this is a jump
                if(Mathf.Abs(x2-x2) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];

                    if(p!= null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p);
                    }
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;

                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
        }

        /*
        //check distance max 1 (to be removed after jump over pieces is implemented)
        if ((Math.Abs(x2 - x1) > 1) || (Math.Abs(y2 - y1) > 1))
            return;
        //check if we are out of bounds
        if (x2 < 0 || x1 >= pieces.Length || y2 < 0 || y2 >= pieces.Length)
            return;
        //if there is a selected piece

        //if endDrag is occupied
        if (pieces[x2, y2] ) //!= null
        return;
        //ensure diagonal move only
        if (((x2 - x1) % 2 == 0) || ((y2 - y1) % 2 == 0))
            return;

        //if Valid move
        MovePiece(selectedPiece, x2, y2);
        pieces[x1, y1] = null;
        pieces[x2, y2] = selectedPiece;*/
    }

    private void EndTurn()
    {
        selectedPiece = null;
        startDrag = Vector2.zero;

        isWhiteTurn = !isWhiteTurn;

        CheckVictory();
    }

    private void CheckVictory()
    {

    }

    private void GenerateBoard()
    {
        //Generate white team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate the piece
                GeneratePiece((oddRow) ? x : x+1, y);
            }

        }

        //Generate black team
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate the piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }

        }
    }
    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }

}