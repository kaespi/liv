#include "FileSystemWrapperFactory.h"

#include "DirWrapper.h"
#include "FileSystemWrapper.h"

#include <memory>

std::unique_ptr<IDir> FileSystemWrapperFactory::createDirClass() const
{
    return std::make_unique<DirWrapper>();
}

std::unique_ptr<IFileSystem> FileSystemWrapperFactory::createFileSystemClass() const
{
    return std::make_unique<FileSystemWrapper>();
}
