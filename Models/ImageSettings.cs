using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesToBinary.Models;

public class ImageSettings
{
    public required string RootPath { get; set; }
    public required List<TargetTable> Tables { get; set; }
}

public class TargetTable
{
    public required string TableName { get; set; }
    public required string ImagePathColumn { get; set; }
    public required string BinaryColumn { get; set; }
    public required string ImagesFolderPath { get; set; }
}
