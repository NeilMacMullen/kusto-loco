using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Create a message
public class LayoutChangedMessage(int layout) : ValueChangedMessage<int>(layout);
