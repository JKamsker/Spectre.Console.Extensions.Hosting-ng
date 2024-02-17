// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

internal class CommandLineArgsProvider : IArgsProvider
{
    public string[] GetArgs()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Skip(1).ToArray();
    }
}

internal class StoredArgsProvider : IArgsProvider
{
    private readonly string[] _args;

    public StoredArgsProvider(string[] args)
    {
        _args = args;
    }

    public string[] GetArgs()
    {
        return _args;
    }
}

public interface IArgsProvider
{
    string[] GetArgs();
}
