using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.LogIn.UserInfo
{
    internal class DetermineCurrentUser
    {
        // Function to determine the current user engaging in whatever activity within the POS system
        // Will pop up and request a user pin to verify identity durring specific actions (like finalizing a sale, etc.)
        // Will work by showing a popup window that requests a user pin, then checks the pin against the database to find the user associated with that pin

        // For now, return a placeholder user ID 

        public int employeeId()
        {
            // Placeholder user ID
            return 1;
        }

    }
}
