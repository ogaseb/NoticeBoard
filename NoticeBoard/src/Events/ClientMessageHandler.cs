﻿using NoticeBoard.Packets;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace NoticeBoard.Events
{
    internal class ClientMessageHandler
    {
        private NoticeBoardModSystem modSystem = NoticeBoardModSystem.getModInstance();
        private NoticeBoardMainWindowGui messageBoardGui;

        public void SetMessageHandlers()
        {
            NoticeBoardModSystem.getCAPI().Network.GetChannel("noticeboard").SetMessageHandler<ResponseAllMessages>(OnServerMessagesReceived);
        }

        private void OnServerMessagesReceived(ResponseAllMessages packet)
        {
            if (messageBoardGui == null || !messageBoardGui.IsOpened())
            {
                messageBoardGui = new NoticeBoardMainWindowGui("NoticeBoardGui", packet, NoticeBoardModSystem.getCAPI());
                messageBoardGui.TryOpen();
            }

            messageBoardGui.UpdateMessages(packet.Messages);
        }
    }
}
