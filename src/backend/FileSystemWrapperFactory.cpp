#include "FileSystemWrapperFactory.h"

#include "DirWrapper.h"

#include <memory>

std::unique_ptr<IDir> FileSystemWrapperFactory::createDirClass() const
{
    return std::make_unique<DirWrapper>();
}
