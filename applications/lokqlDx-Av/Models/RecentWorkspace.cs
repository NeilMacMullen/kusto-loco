using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.Models;
public class RecentWorkspace(string name, string path)
{
    public string Name { get; } = name;
    public string Path { get; } = path;
}
