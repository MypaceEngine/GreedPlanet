using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class ScrollTilemap : MonoBehaviour {

    [SerializeField]
    private Tilemap tileMap;

    [SerializeField]
    private Tile[] tiles;


    private int[,] cells = {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 1, },
        { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 6, 2, 1, },
        { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 2, 7, 2, 2, },
        { 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 5, 1, 1, 1, 1, 1, 2, 2, 2, },
        { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 2, 1, },
        { 0, 0, 0, 0, 0, 1, 1, 1, 2, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, },
        { 0, 0, 0, 0, 1, 2, 2, 2, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, },
        { 0, 0, 0, 0, 1, 2, 2, 4, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 1, 0, },
        { 0, 0, 0, 1, 2, 2, 1, 4, 3, 3, 2, 1, 1, 0, 0, 0, 0, 1, 1, 0, },
        { 0, 0, 0, 0, 1, 2, 1, 3, 4, 3, 3, 1, 0, 0, 0, 0, 0, 1, 0, 0, },
        { 0, 0, 0, 0, 1, 1, 2, 1, 3, 3, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 1, 1, 1, 3, 3, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 1, 8, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
    };

    float moveTime = 0.0f;
    Vector2Int playerPosition = new Vector2Int( 9, 7 );

	// Use this for initialization
	void Start () {
        MapUpdate( playerPosition );
	}

    int x_dir = 0;
    int y_dir = 0;

	// Update is called once per frame
	void Update () {

        if ( ( x_dir == 0 ) && ( y_dir == 0 ) ) {
            if ( Input.GetAxis( "Horizontal" ) > 0.0f ) {
                x_dir = 1;
                moveTime = 0.0f;
            }
            if ( Input.GetAxis( "Horizontal" ) < 0.0f ) {
                x_dir = -1;
                moveTime = 0.0f;
            }
        }
        if ( ( x_dir == 0 ) && ( y_dir == 0 ) ) {

            if ( Input.GetAxis( "Vertical" ) > 0.0f ) {
                y_dir = -1;
                moveTime = 0.0f;
            }
            if ( Input.GetAxis( "Vertical" ) < 0.0f ) {
                y_dir = 1;
                moveTime = 0.0f;
            }
        }

        if ( ( x_dir != 0 ) || ( y_dir != 0 ) ) {

            moveTime += Time.deltaTime;

            if ( moveTime > 0.4f ) {

                playerPosition.x += x_dir;
                playerPosition.y += y_dir;

                if ( playerPosition.x > 19 ) {
                    playerPosition.x = 0;
                }
                if ( playerPosition.x < 0 ) {
                    playerPosition.x = 19;
                }

                if ( playerPosition.y > 19 ) {
                    playerPosition.y = 0;
                }
                if ( playerPosition.y < 0 ) {
                    playerPosition.y = 19;
                }

                MapUpdate( playerPosition );

                x_dir = 0;
                y_dir = 0;
                moveTime = 0.0f;
            }
            Camera.main.transform.localPosition = new Vector3( 10 + Mathf.Clamp( moveTime / 0.4f, 0.0f, 1.0f ) * x_dir, -( 5 + Mathf.Clamp( moveTime / 0.4f, 0.0f, 1.0f ) * y_dir ), -10 );

        }



	}

    void MapUpdate( Vector2Int pos ) {

        Vector3Int v = Vector3Int.zero;

        for ( int y = 0; y < 12; ++y ) {
            for ( int x = 0; x < 20; ++x ) {

                int cell_x = pos.x - 10 + x;
                int cell_y = pos.y - 6 + y;

                if ( cell_x < 0 ) {
                    cell_x = 20 + cell_x;
                }
                if ( cell_x > 19 ) {
                    cell_x = cell_x - 20;
                }

                if ( cell_y < 0 ) {
                    cell_y = 20 + cell_y;
                }
                if ( cell_y > 19 ) {
                    cell_y = cell_y - 20;
                }

                var field_tile = tiles[ cells[ cell_y, cell_x ] ];

                tileMap.SetTile( new Vector3Int( x, -y, 0 ), field_tile );
            }
        }

    }

}
