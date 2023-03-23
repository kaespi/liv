#include "BackendManager.h"

#include <QImageReader>

BackendManager::BackendManager(const QString& file)
{
    auto supportedFormats = QImageReader::supportedImageFormats();
    m_ptrFileSystemWalker = std::make_unique<FileSystemWalker>(file, supportedFormats);
    m_ptrImgProvider = new ImgProvider(m_ptrFileSystemWalker.get());
}

BackendManager::~BackendManager()
{
    if (not m_imgProviderRequested && m_ptrImgProvider)
    {
        delete m_ptrImgProvider;
    }
}

ImgProvider* BackendManager::requestImgProvider()
{
    m_imgProviderRequested = true;
    return m_ptrImgProvider;
}
