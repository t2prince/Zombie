using System;
using Jamcat.Core;
using Jamcat.Managers.Item;
using Jamcat.Managers.Monster;
using Jamcat.Managers.Player;
using Jamcat.Managers.Weapon;
using Jjamcat.Util;

public class AppDirector : Singleton<AppDirector>
{
    private void Awake()
    {
        InitManager();
        InitPlayer();
    }

    private void InitManager()
    {
        
        
    }

    private void InitPlayer()
    {
        
    }
}
