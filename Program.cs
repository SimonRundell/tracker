/**
 * AtRiskTracker - WinForms front-end for the AtRiskRegister PHP/MySQL back-end.
 *
 * Entry point. Enables visual styles and starts the login form.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;

namespace AtRiskTracker
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.LoginForm());
        }
    }
}
