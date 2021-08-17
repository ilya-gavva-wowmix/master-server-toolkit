﻿using MasterServerToolkit.Logging;
using MasterServerToolkit.Networking;
using System.Collections.Generic;
using System.Linq;

namespace MasterServerToolkit.MasterServer
{
    public delegate void FindGamesCallback(List<GameInfoPacket> games);
    public delegate void GetRegionsCallback(List<string> regions);
    public delegate void GetPlayersCallback(List<string> players);

    public class MstMatchmakerClient : MstBaseClient
    {
        /// <summary>
        /// List of the last loaded games
        /// </summary>
        public List<GameInfoPacket> Games { get; private set; }
        /// <summary>
        /// List of regions at which all the rooms are registered
        /// </summary>
        public List<string> Regions { get; private set; }

        public MstMatchmakerClient(IClientSocket connection) : base(connection) { }

        /// <summary>
        /// Gets list of regions at which all the rooms are registered
        /// </summary>
        /// <param name="callback"></param>
        public void GetRegions(GetRegionsCallback callback)
        {
            GetRegions(callback, Connection);
        }

        /// <summary>
        /// Gets list of regions at which all the rooms are registered
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="connection"></param>
        public void GetRegions(GetRegionsCallback callback, IClientSocket connection)
        {
            if (!connection.IsConnected)
            {
                Regions = new List<string>();
                Logs.Error("Not connected");
                callback?.Invoke(Regions);
                return;
            }

            connection.SendMessage((short)MstMessageCodes.GetRegionsRequest, (status, response) =>
            {
                if (status != ResponseStatus.Success)
                {
                    Regions = new List<string>();
                    Logs.Error(response.AsString("Unknown error while requesting a list of regions"));
                    callback?.Invoke(Regions);
                    return;
                }

                Regions = response.Deserialize(new RegionsPacket()).Regions;
                callback?.Invoke(Regions);
            });
        }

        /// <summary>
        /// Retrieves a list of all public games
        /// </summary>
        /// <param name="callback"></param>
        public void FindGames(FindGamesCallback callback)
        {
            FindGames(new MstProperties(), callback, Connection);
        }

        /// <summary>
        /// Retrieves a list of public games, which pass a provided filter.
        /// (You can implement your own filtering by extending modules or "classes" 
        /// that implement <see cref="IGamesProvider"/>)
        /// </summary>
        public void FindGames(MstProperties filter, FindGamesCallback callback)
        {
            FindGames(filter, callback, Connection);
        }

        /// <summary>
        /// Retrieves a list of public games, which pass a provided filter.
        /// (You can implement your own filtering by extending modules or "classes" 
        /// that implement <see cref="IGamesProvider"/>)
        /// </summary>
        public void FindGames(MstProperties filter, FindGamesCallback callback, IClientSocket connection)
        {
            if (!connection.IsConnected)
            {
                Games = new List<GameInfoPacket>();
                Logs.Error("Not connected");
                callback?.Invoke(Games);
                return;
            }

            connection.SendMessage((short)MstMessageCodes.FindGamesRequest, filter.ToBytes(), (status, response) =>
            {
                if (status != ResponseStatus.Success)
                {
                    Games = new List<GameInfoPacket>();
                    Logs.Warn(response.AsString("Unknown error while requesting a list of games"));
                    callback?.Invoke(Games);
                    return;
                }

                Games = response.DeserializeList(() => new GameInfoPacket()).ToList();
                callback?.Invoke(Games);
            });
        }
    }
}