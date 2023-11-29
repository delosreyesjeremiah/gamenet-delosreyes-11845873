using System.Numerics;
using UnityEngine;

public class Constants
{
    #region Levels

    public const string LEVEL_FREE_FOR_ALL_DEATHMATCH = "FreeForAllDeathmatchScene";
    public const string LEVEL_ELEMENTAL_DOMINATION = "ElementalDominationScene";

    #endregion

    #region Game Mode Properties

    public const string GAME_MODE = "gm";
    public const string GAME_MODE_FREE_FOR_ALL = "fa";
    public const string GAME_MODE_ELEMENTAL_DOMINATION = "do";
    public const string GAME_MODE_FREE_FOR_ALL_HEADING = "Free-For-All Deathmatch";
    public const string GAME_MODE_ELEMENTAL_DOMINATION_HEADING = "Elemental Domination";

    #endregion

    #region Player Properties

    public const string PLAYER_READY = "isPlayerReady";
    public const string PLAYER_SELECTION_INDEX = "playerSelectionIndex";

    #endregion

    #region Room Properties

    public const int MAX_PLAYERS_IN_ROOM = 3;
    public const int MIN_ROOM_RANGE_INDEX = 1000;
    public const int MAX_ROOM_RANGE_INDEX = 10000;

    #endregion

    #region Elemental Stats

    public const float FIRE_ELEMENTAL_DAMAGE = 40.0f;
    public const float FIRE_ELEMENTAL_DEFENSE = 5.0f;
    public const float FIRE_ELEMENTAL_SPEED = 10.0f;

    public const float WATER_ELEMENTAL_DAMAGE = 20.0f;
    public const float WATER_ELEMENTAL_DEFENSE = 10.0f;
    public const float WATER_ELEMENTAL_SPEED = 20.0f;

    public const float EARTH_ELEMENTAL_DAMAGE = 30.0f;
    public const float EARTH_ELEMENTAL_DEFENSE = 15.0f;
    public const float EARTH_ELEMENTAL_SPEED = 5.0f;

    #endregion

    #region Multiplier Matchups

    public const float STRENGTH_MULTIPLIER = 1.20f;
    public const float WEAKNESS_MULTIPLIER = 0.8f;

    #endregion;

    #region Elemental Zones

    public const int NUMBER_OF_ZONES = 4;

    #endregion
}
