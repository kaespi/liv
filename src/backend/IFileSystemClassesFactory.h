#pragma once

#include "IDir.h"
#include "IFileSystem.h"

#include <memory>

class IFileSystemClassesFactory
{
  public:
    virtual ~IFileSystemClassesFactory(){};
    virtual std::unique_ptr<IDir> createDirClass() const = 0;
    virtual std::unique_ptr<IFileSystem> createFileSystemClass() const = 0;
};
