#pragma once

#include "IDir.h"

#include <memory>

class IFileSystemClassesFactory
{
public:
    virtual std::unique_ptr<IDir> createDirClass() const = 0;
};
