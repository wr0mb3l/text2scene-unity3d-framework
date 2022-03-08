using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginData
{
    public string Username { get; private set; }

    public string Password { get; private set; }

    /// <summary>
    /// /// Stores login data
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">The password as string, it will be automatically returned as md5</param>
    public LoginData(string username, string md5Password)
    {
        Username = username;
        Password = md5Password;
    }
}