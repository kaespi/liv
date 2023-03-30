#pragma once

#include <QString>

class IFileSystemWalker
{
  public:
    virtual ~IFileSystemWalker() = default;

    virtual QString getCurrentFile() const = 0;
    virtual QString getNextFile() = 0;
    virtual QString getPrevFile() = 0;
};
