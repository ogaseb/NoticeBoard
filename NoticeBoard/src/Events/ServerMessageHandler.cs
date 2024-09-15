﻿using NoticeBoard.src.Packets;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using NoticeBoard.Extensions;


namespace NoticeBoard.src.Events
{
    internal class ServerMessageHandler
    {
        private readonly SQLiteHandler db = new SQLiteHandler();
        public void SetMessageHandlers()
        {
            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SetMessageHandler<RequestAllMessages>(OnPlayerRequestAllMessages);
            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SetMessageHandler<PlayerSendMessage>(OnPlayerSendMessage);
            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SetMessageHandler<PlayerRemoveMessage>(OnPlayerRemoveMessage);
            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SetMessageHandler<PlayerDestroyNoticeBoard>(OnPlayerDestroyNoticeBoard);
            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SetMessageHandler<PlayerCreateNoticeBoard>(OnPlayerCreateNoticeBoard);
        }

        private void OnPlayerDestroyNoticeBoard(IServerPlayer player, PlayerDestroyNoticeBoard packet)
        {
            db.DeleteNoticeBoard(packet.BoardId);
        }


        private void OnPlayerCreateNoticeBoard(IServerPlayer player, PlayerCreateNoticeBoard packet)
        {
            db.CreateNoticeBoard(packet);
        }

        private void OnPlayerRemoveMessage(IServerPlayer player, PlayerRemoveMessage packet)
        {
            db.DeleteMessage(packet.MessageId);
        }

        private void OnPlayerSendMessage(IServerPlayer player, PlayerSendMessage packet)
        {

            db.InsertMessage(packet, player.PlayerName);

            var proximityGroup = NoticeBoardModSystem.getSAPI().Groups.GetPlayerGroupByName("Proximity").Uid;
            if (proximityGroup != 0)
            {
                NoticeBoardObject noticeBoard = db.GetBoardData(packet.BoardId);
                string rpNickName = NoticeBoardModSystem.getSAPI().GetPlayerByUID(player.PlayerUID).GetModData("BASIC_NICKNAME", player.PlayerName);

                foreach (IServerPlayer serverPlayer in NoticeBoardModSystem.getSAPI().World.AllOnlinePlayers)
                {
                    player.SendMessage(proximityGroup, $"<strong>{rpNickName}</strong> {Lang.Get("noticeboard:new-notice-proximity-message")} ({noticeBoard.Pos})", EnumChatType.Notification);
                }
            }
        }

        private void OnPlayerRequestAllMessages(IServerPlayer player, RequestAllMessages packet)
        {
            List<Message> messages = db.GetAllMessages(packet.BoardId);
            messages.Reverse();

            ResponseAllMessages responsePacket = new ResponseAllMessages
            {
                BoardId = packet.BoardId,
                PlayerId = packet.PlayerId,
                Messages = messages
            };

            NoticeBoardModSystem.getSAPI().Network.GetChannel("noticeboard").SendPacket(responsePacket, player);
        }
    }
}
