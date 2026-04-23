using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KapwaKuha.ViewModels
{
    public class ChatListDesignViewModel
    {
        public ObservableCollection<ChatUserRow> ChatUsers { get; } = new()
        {
            new ChatUserRow { UserId="D001", DisplayName="Juan Dela Cruz",  LastMessage="Can I pick up tomorrow?",    UnreadCount=2 },
            new ChatUserRow { UserId="D002", DisplayName="Maria Santos",    LastMessage="Thanks for the donation!",   UnreadCount=0 },
            new ChatUserRow { UserId="D003", DisplayName="Pedro Reyes",     LastMessage="Where is the pickup point?", UnreadCount=1 },
        };
    }
}
