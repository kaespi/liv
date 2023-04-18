#pragma once

#include "IFileSystem.h"

#include <QDir>

class FileSystemWrapper : public IFileSystem
{
  public:
    FileSystemWrapper() = default;
    virtual ~FileSystemWrapper() = default;

    virtual bool fileExists(const QString& path) const override;
};
