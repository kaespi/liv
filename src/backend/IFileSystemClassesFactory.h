#pragma once

#include "IDir.h"

#include <memory>

class IFileSystemClassesFactory
{
  public:
    virtual ~IFileSystemClassesFactory(){};
    virtual std::unique_ptr<IDir> createDirClass() const = 0;
};
