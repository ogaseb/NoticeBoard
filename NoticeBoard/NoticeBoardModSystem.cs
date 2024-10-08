﻿using Microsoft.Data.Sqlite;
using NoticeBoard.Configs;
using NoticeBoard.Events;
using NoticeBoard.Packets;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NoticeBoard
{
    public class NoticeBoardModSystem : ModSystem
    {
        public static NoticeBoardModSystem modInstance;
        static ICoreServerAPI sapi;
        static ICoreClientAPI capi;
        static SQLiteDatabase databaseHandler;

        public static ModConfig config;
        private const string ConfigName = "noticeboard/noticeboard.json";
        public NoticeBoardModSystem()
        {
            modInstance = this;
        }

        public static ICoreServerAPI getSAPI()
        {
            return sapi;
        }

        public static ICoreClientAPI getCAPI()
        {
            return capi;
        }

        public SQLiteDatabase getDatabaseHandler()
        {
            return databaseHandler;
        }

        public static NoticeBoardModSystem getModInstance()
        {
            return modInstance;
        }

        public static ModConfig getConfig()
        {
            return config;
        }
        public static void LoadDatabase()
        {
            try
            {
                databaseHandler = new SQLiteDatabase();
            }
            catch (SqliteException ex)
            {
                sapi.Logger.Error("loadDatabase:" + ex.Message);
            }
        }

        private void LoadConfig()
        {
            try
            {
                config = sapi.LoadModConfig<ModConfig>(ConfigName);
            }
            catch (Exception)
            {
                sapi.Server.LogError("NoticeBoard: Failed to load mod config!");
                return;
            }

            if (config == null)
            {
                sapi.Server.LogNotification("NoticeBoards: non-existant modconfig at 'ModConfig/noticeboard" + ConfigName +
                                           "', creating default...");
                config = new ModConfig();
                sapi.StoreModConfig(config, ConfigName);
            }
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("NoticeBoardBlock", typeof(NoticeBoardBlock));
            api.RegisterBlockEntityClass("NoticeBoardBlockEntity", typeof(NoticeBoardBlockEntity));


            api.Network.RegisterChannel("noticeboard")
            .RegisterMessageType(typeof(RequestAllMessages))
            .RegisterMessageType(typeof(ResponseIsActive))
            .RegisterMessageType(typeof(ResponseAllMessages))
            .RegisterMessageType(typeof(PlayerSendMessage))
            .RegisterMessageType(typeof(PlayerEditMessage))
            .RegisterMessageType(typeof(PlayerDestroyNoticeBoard))
            .RegisterMessageType(typeof(PlayerCreateNoticeBoard))
            .RegisterMessageType(typeof(PlayerRemoveMessage));

            api.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

            new ClientMessageHandler().SetMessageHandlers();
        }


        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            sapi = api;

            sapi.Event.ServerRunPhase(EnumServerRunPhase.ModsAndConfigReady, () => 
            {
                LoadConfig();
                LoadDatabase();
                new ServerMessageHandler().SetMessageHandlers();
            });
        }

       
    }
}
