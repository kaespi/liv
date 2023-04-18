#pragma once

#include <QDir>
#include <QString>

class IFileSystem
{
  public:
    virtual ~IFileSystem() = default;
    virtual bool fileExists(const QString& path) const = 0;
};
