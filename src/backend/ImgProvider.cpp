#include "ImgProvider.h"

ImgProvider::ImgProvider(FileSystemWalker* const ptrFileSystemWalker)
    : QQuickImageProvider(QQuickImageProvider::Pixmap), m_ptrFileSystemWalker(ptrFileSystemWalker)
{}

QPixmap ImgProvider::requestPixmap(const QString& imgName, QSize* size, const QSize& /*requestedSize*/)
{
    QPixmap pixmap;
    if (imgName.contains('_'))
    {
        auto ixUnderscore = imgName.lastIndexOf('_');
        auto command = imgName.last(imgName.size() - ixUnderscore - 1);
        if (command == "next")
        {
            pixmap.load(m_ptrFileSystemWalker->getNextFile());
        }
        else if (command == "prev")
        {
            pixmap.load(m_ptrFileSystemWalker->getPrevFile());
        }
        else
        {
            qWarning("Unknown command %s", command.toLatin1().data());
            pixmap.load(m_ptrFileSystemWalker->getCurrentFile());
        }
    }
    else
    {
        pixmap.load(m_ptrFileSystemWalker->getCurrentFile());
    }

    *size = pixmap.size();

    return pixmap;
}
