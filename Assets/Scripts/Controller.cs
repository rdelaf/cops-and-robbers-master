using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;

    void Start()
    {
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }

    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();
            }
        }

        cops[0].GetComponent<CopMove>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile = Constants.InitialRobber;
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //Inicializamos a 0
        for (int i = 0; i <= Constants.NumTiles - 1; i++)
        {
            for (int j = 0; j <= Constants.NumTiles - 1; j++)
            {
                matriu[i, j] = 0;
                Debug.Log(matriu[i, j]);
            }
        }

        //Las casillas adyacentes a esta tendrán una distancia de 1/-1 en horizontal
        //y de 8/-8 posiciones en vertical
        for (int file = 0; file <= Constants.NumTiles - 1; file++)
        {
            for (int col = 0; col <= Constants.NumTiles - 1; col++)
            {
                if (col - file == 8 || col - file == -8 || (col - file == -1 && file % 8 != 0) || (col - file == 1 && col % 8 != 0)) matriu[file, col] = 1;
                else matriu[file, col] = 0;
            }
        }

        //Decimos a cada casilla cual es adyacente a ella comprobándolo en matriu[]
        for (int i = 0; i <= tiles.Length - 1; i++)
        {
            for (int col = 0; col <= Constants.NumTiles - 1; col++)
            {
                if (matriu[i, col] == 1) tiles[i].adjacency.Add(col);
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;
                break;
        }
    }

    public void ClickOnTile(int t)
    {
        clickedTile = t;

        switch (state)
        {
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile = tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;

                    state = Constants.TileSelected;
                }
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }
    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        //Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        int randomLenght = tiles[cops[clickedCop].GetComponent<CopMove>().currentTile].adjacency.Count;
        int randomTileRobber = tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[Random.Range(0, randomLenght)];

        //Actualizamos la variable currentTile del caco a la nueva casilla
        robber.GetComponent<RobberMove>().currentTile = randomTileRobber;

        //Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        randomLenght = tiles[cops[clickedCop].GetComponent<CopMove>().currentTile].adjacency.Count;
        randomTileRobber = tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[Random.Range(0, randomLenght)];

        //Movemos al caco a esa casilla
        robber.GetComponent<RobberMove>().MoveToTile(tiles[randomTileRobber]);

        //Actualizamos la variable currentTile del caco a la nueva casilla
        robber.GetComponent<RobberMove>().currentTile = randomTileRobber;
    }

    public void EndGame(bool end)
    {
        if (end)
        {
            finalMessage.text = "You Win!";
        }
        else
        {
            finalMessage.text = "You Lose!";
            playAgainButton.interactable = true;
            state = Constants.End;
        }
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);

        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        int indexcurrentTile;

        if (cop == true) indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j <= tiles[cops[clickedCop].GetComponent<CopMove>().currentTile].adjacency.Count - 1; j++)
            {
                if (tiles[i].numTile == tiles[cops[clickedCop].GetComponent<CopMove>().currentTile].adjacency[j])
                {
                    tiles[i].selectable = true;
                    for (int x = 0; x <= tiles[i].adjacency.Count - 1; x++)
                    {
                            tiles[tiles[i].adjacency[x]].selectable = true;
                    }
                }
            }
        }
    }
}
