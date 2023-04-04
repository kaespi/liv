#include "BackendManager.h"

#include "FileSystemWrapperFactory.h"

#include <QImageReader>

BackendManager::BackendManager(const QString& file)
{
    auto supportedFormats = QImageReader::supportedImageFormats();
    std::unique_ptr<FileSystemWrapperFactory> ptrFsWrapperFactory =
        std::make_unique<FileSystemWrapperFactory>();
    m_ptrFileSystemWalker =
        std::make_unique<FileSystemWalker>(file, ptrFsWrapperFactory.get(), supportedFormats);
    m_ptrImgProvider = new ImgProvider(m_ptrFileSystemWalker.get());
}

BackendManager::~BackendManager()
{
    if (not m_imgProviderRequested && (m_ptrImgProvider != nullptr))
    {
        delete m_ptrImgProvider;
    }
}

ImgProvider* BackendManager::requestImgProvider()
{
    m_imgProviderRequested = true;
    return m_ptrImgProvider;
}
