#pragma once

#include "IFileSystemClassesFactory.h"

class FileSystemWrapperFactory : public IFileSystemClassesFactory
{
  public:
    FileSystemWrapperFactory() = default;
    virtual ~FileSystemWrapperFactory() = default;

    virtual std::unique_ptr<IDir> createDirClass() const override;
};
